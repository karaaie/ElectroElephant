module ElectroElephant.Api

open ElectroElephant.Common
open ElectroElephant.Connection
open ElectroElephant.MetadataRequest
open ElectroElephant.MetadataResponse
open ElectroElephant.Model
open ElectroElephant.Request
open ElectroElephant.Response
open ElectroElephant.SocketHelpers
open ElectroElephant.StreamHelpers
open Logary
open Microsoft.FSharp.Core.Operators
open System
open System.Collections.Generic
open System.IO
open System.Net.Sockets
open System.Threading

let logger = Logging.getLoggerByName("EE.API")

/// This should be configurable at some point.
let read_buffer_size = 1024

/// start the correlation id on Zero, this is only interesting on a session basis
let mutable correlation_id = 0

/// get and increment the correlation id, not thread safe currently.
let next_correlation_id() = 
  correlation_id <- correlation_id + 1
  correlation_id

/// This map will contain all the correlation IDs and the corresponding API which was 
/// called. This will make it easier to deserialize the request.
let correlatation_map = new Dictionary<CorrelationId, ApiKeys>()

/// This clients sender ID, Should be configurable and correlate to the applications
/// name/type w/e
let get_client_id = "kamils client"

/// Correlate an request type with an ApiKey
let api_key (req_t : RequestTypes) = 
  match req_t with
  | RequestTypes.Metadata _ ->  ApiKeys.MetadataRequest
  | RequestTypes.Produce _ -> ApiKeys.ProduceRequest
  | RequestTypes.Fetch _ ->  ApiKeys.FetchRequest
  | RequestTypes.Offsets _ -> ApiKeys.OffsetRequest
  | RequestTypes.ConsumerMetadata _ -> ApiKeys.ConsumerMetadataRequest
  | RequestTypes.OffsetCommit _ ->  ApiKeys.OffsetCommitRequest
  | RequestTypes.OffsetFetch _ -> ApiKeys.OffsetFetchRequest

/// <summary>
///  This is the callback used while sending data to a kafka broker
/// </summary>
/// <param name="result"></param>
let rec private send_callback (result : IAsyncResult) = 
  try 
    let state = result.AsyncState :?> SendState
    let bytes_sent = end_send state result
    state.sent_bytes <- state.sent_bytes + bytes_sent
    if state.sent_bytes >= state.payload.Length then 
      /// we're done
      LogLine.info "successfully sent a message to kafka"
      |> LogLine.setData "sent bytes" state.sent_bytes
      |> Logger.log logger
    else 
      // not all data is sent to the socket, continue sending data
      begin_send state send_callback |> ignore
  with /// TODO
       /// this needs much more error handling, we need to be able to handle the following:
       /// Leader for partition changed  -> attempt to rebuild metadata
       /// Broker down                   -> attempt to rebuild metadata
       /// too large datasize            -> nothing we can do than log it as an error
       /// General Transmission error    -> do a retry
       ex -> 
    LogLine.error "failed when sending data"
    |> LogLine.setExn ex
    |> Logger.log logger

/// Determines how many bytes to read on the next 
/// receive attempt.
let private next_read_size state = 
  match (state.msg_size - state.total_read_bytes) with
  // This is the case when we have looots of data to read still.
  // so lets attempt to fill our buffer.
  | x when x >= state.buffer.Length -> state.buffer.Length
  // this happens when we have a small message or are at the end of the current message
  // so we only read the remaning bytes.
  | x when x < state.buffer.Length -> x
  | _ -> failwith "shouldn't happen"

/// Finishes the receive operation by transforming the state object to a Response record
/// and then calls the callback with it.
let private finish_receive state = 
  //we have all the data we need in order to deserialize the response.
  let api_key = correlatation_map.[state.corr_id]
  //deserialize the entire response
  use memStream = new MemoryStream(state.stream.ToArray())
  let resp = Response.deserialize api_key memStream
  //fire the callback.
  state.callback resp

let kafka_broker_shutdown_socket (state : ReceiveState) = 
  // The EndReceive method will read as much data as is available up to the number 
  // of bytes you specified in the size parameter of the BeginReceive method.
  // if the remote host shuts down the Socket connection with the Shutdown 
  // method, and all available data has been received, the EndReceive 
  // method will complete immediately and return zero bytes.
  let err_msg = "broker shutdown the socket"
  LogLine.error err_msg
  |> LogLine.setData "receive state" state
  |> Logger.log logger
  raise (new Exception(err_msg))

/// Reads chunks from the socket until all data is gathered.
/// and handles errors that might occur.
let rec private receive_response_payload (result : IAsyncResult) = 
  try 
    let state = result.AsyncState :?> ReceiveState
    let bytes_received = end_receive state result
    if bytes_received = 0 then kafka_broker_shutdown_socket state
    else 
      // update how far we've read
      state.total_read_bytes <- state.total_read_bytes + bytes_received
      // write the current buffer to the stream.
      state.stream.Write(state.buffer, 0, bytes_received)
      if state.msg_size = state.total_read_bytes then finish_receive state
      else 
        begin_receive state (next_read_size state) receive_response_payload 
        |> ignore
  with ex -> 
    LogLine.error "failed while reading data, this needs to be handled."
    |> LogLine.setExn ex
    |> Logger.log logger

let handle_socket_error (ex : SocketException) = 
  LogLine.error 
    "failed while reading data, have a look at http://msdn.microsoft.com/en-us/library/windows/desktop/ms740668%28v=vs.85%29.aspx"
  |> LogLine.setData "socket error type" ex.ErrorCode
  |> LogLine.setExn ex
  |> Logger.log logger

/// this method handles the initial response from the kafka broker. 
/// it grabs the size of the response so we know how much more we need to read.
let private receive_response_size (result : IAsyncResult) = 
  try 
    Logger.info logger "receive response size called"
    let state = result.AsyncState :?> ReceiveState

    LogLine.info "data stuck in buffer 1"
      |> LogLine.setData "read buffer" state.socket.Available
      |> Logger.log logger

    let bytes_received = end_receive state result

    LogLine.info "data stuck in buffer 2"
      |> LogLine.setData "read buffer" state.socket.Available
      |> LogLine.setData "read bytes" bytes_received
      |> Logger.log logger

    if bytes_received = 0 then kafka_broker_shutdown_socket state
    else 
      // the stream should contain atleast the size and probably some of the response
      // lets find out the size
      let payload_size = BitConverter.ToInt32(state.buffer.[..3] |> correct_endianess , 0)
      LogLine.info "got msg size info"
      |> LogLine.setData "size" payload_size
      |> Logger.log logger

      let payload_state = 
        { state with msg_size = payload_size
                     total_read_bytes = 0 }
      begin_receive payload_state payload_size receive_response_payload 
      |> ignore
  with
    | :? SocketException as ex -> handle_socket_error ex
    | ex -> LogLine.error "failed while reading data" 
            |> LogLine.setExn ex
            |> Logger.log logger

/// <summary>
///   Mission: 
///     A. wrap the specific request in to a format which kafka understands
///     B. Set up the correlation id and callback which can be called out-of-band when the response arrives
///     C. Serialize the message
///     E. Transmitt the message to the broker
///     F. Check the transmission result, did it succeed or fail?
///        depending on what response we get we might have to rebuild our metadata set
///        or simply try to retransmitt the message.
///     H. Do the BeginReceive.
///  Premises:
///    = Outgoing requests will be buffered in the Socket layer because kafka only accepts one in-flight message at a time.
///      Implications/Simplifications:
///       Once we fire the BeginSend on the socket to the current broker that Request might be waiting in the OS socket layer
///      until kafka starts to accept the next request. Thus directly after we do the BeginSend we can do the BeginReceive because the callback in it
///        will fire as soon as we have data available to read ( that is, kafka has finished processing our request and starts sending the response to us)
///
/// </summary>
/// <param name="req_t">The type of request that we want to transmitt</param>
/// <param name="callback">The callback which should be called when we get a response</param>
/// <param name="topic">which topic the message is for</param>
/// <param name="key">the key decides which partition the message is for</param>
/// <param name="value">what we want to publish</param>
let private send_request<'RequestType> (req_t : RequestTypes) 
    (callback : Response -> unit) (socket : Socket) = 
  LogLine.debug "meta req"
  |> LogLine.setData "meta req" req_t
  |> Logger.log logger
  //B. 
  let current_correlation_id = next_correlation_id()


  correlatation_map.Add(current_correlation_id, api_key req_t)
  //A. 
  let req = 
    { api_version = 0s
      api_key = int16 (api_key req_t)
      correlation_id = current_correlation_id
      client_id = get_client_id
      request_type = req_t }

  LogLine.debug "socket info"
  |> LogLine.setData "connected" socket.Connected
  |> Logger.log logger

  //C. serialize the envelop
  use stream = new MemoryStream()
  ElectroElephant.Request.serialize req stream

  //this is the request
  let payload_state = 
    { socket = socket
      payload = stream.ToArray()
      sent_bytes = 0 }

  //kafka expects to get the size of the request first

  let payload_size = correct_endianess <| BitConverter.GetBytes(payload_state.payload.Length)
  let msg_size_state =
      { socket = socket
        payload =  payload_size
        sent_bytes = 0 }

  LogLine.info "msg size info"
  |> LogLine.setData "normal size" payload_state.payload.Length
  |> LogLine.setData "converted size" (BitConverter.ToInt32(msg_size_state.payload |> correct_endianess,0))
  |> LogLine.setData "is Little Endian" BitConverter.IsLittleEndian
  |> Logger.log logger
  //E. transmitt the request
  // lets ignore the async result since that object will be sent to the async callback, we'll handle errors
  // and other stuff there instead.
  begin_send msg_size_state send_callback |> ignore
  begin_send payload_state send_callback |> ignore
  Logger.info logger "begin send done"
  // Start reading the response, we need the first four bytes in order to know how long the response will be.
  let rec_state = 
    { socket = socket
      buffer = Array.zeroCreate read_buffer_size
      total_read_bytes = 0
      msg_size = sizeof<MessageSize>
      callback = callback
      corr_id = current_correlation_id
      stream = new MemoryStream() }
  Logger.info logger "begin receive called"
  begin_receive rec_state rec_state.msg_size receive_response_size

//let do_produce (topic : string) (key : string) (value : byte list) = ()
//let do_fetch (topic : string) (key : string) (offset : Offset) = ()
//let do_offset_commit = ()
//let do_offset_fetch = ()
//let do_offset = ()
/// <summary>
///   wraps the metaresponse callback so the api will be a bit cleaner.
/// </summary>
/// <param name="orginial">the originial callback</param>
/// <param name="resp">the response gotten from the server</param>
let private metadata_callback_wrapper (orginial : MetadataResponse -> unit) 
    (resp : Response) = 
  match resp.response_type with
  | ResponseTypes.Metadata md -> orginial md
  | wrong -> 
    LogLine.error 
      "expected a metadata response, but got another type of response"
    |> LogLine.setData "incorrect response" wrong
    |> Logger.log logger

let do_metadata_request (hostname : string) (port : int) 
    (topics : string list option) (callback : MetadataResponse -> unit) = 
  //any specific topic that we want to constraint us to?
  let meta_req = 
    match topics with
    | Some tps -> RequestTypes.Metadata({ topic_names = tps })
    | None -> RequestTypes.Metadata({ topic_names = [] })

  Logger.info logger "starting connection"

  let socket = new TcpClient(hostname, port)

  LogLine.debug "tcp client info"
  |> LogLine.setData "host" hostname
  |> LogLine.setData "port" port
  |> Logger.log logger

  let result = 
    (send_request<MetadataRequest> meta_req (metadata_callback_wrapper callback) 
       socket.Client)

  Logger.info logger "waiting for the response.."
  Thread.Sleep(TimeSpan.FromSeconds 5.)
  LogLine.info "data stuck in buffer 3"
  |> LogLine.setData "read buffer" socket.Available
  |> Logger.log logger
  Thread.Sleep(TimeSpan.FromSeconds 5.)
//result.AsyncWaitHandle.WaitOne()