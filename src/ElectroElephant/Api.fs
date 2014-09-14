module ElectroElephant.Api

open ElectroElephant.Common
open ElectroElephant.Connection
open ElectroElephant.Request
open ElectroElephant.Response
open Microsoft.FSharp.Core.Operators
open System.Collections.Generic

open ElectroElephant.SocketHelpers

open System.Threading
open System
open System.IO
open System.Net.Sockets

open Logary

let logger = Logging.getCurrentLogger()


/// This should be configurable at some point.
let read_buffer_size = 1024

/// start the correlation id on Zero, this is only interesting on a session basis
let mutable correlation_id = 0

/// get and increment the correlation id, not thread safe currently.
let next_correlation_id () =
  correlation_id <- correlation_id + 1
  correlation_id



type TransitMetadata<'a> = 
  { /// the correlation id, this is how a request is mapped to a response
    correlation_id : int32
    /// Callback that should be called whenever we get a response to our request
    callback : 'a -> unit }

/// This map will contain all the correlation IDs and the corresponding API which was 
/// called. This will make it easier to deserialize the request.
let correlatation_map = new Dictionary<CorrelationId, ApiKeys>()

/// This clients sender ID, Should be configurable and correlate to the applications
/// name/type w/e
let get_client_id = "kamils client"


/// Correlate an request type with an ApiKey
let api_key (req_t : RequestTypes) = 
  match req_t with
  | RequestTypes.Metadata _ -> int16 ApiKeys.MetadataRequest
  | RequestTypes.Produce _ -> int16 ApiKeys.ProduceRequest
  | RequestTypes.Fetch _ -> int16 ApiKeys.FetchRequest
  | RequestTypes.Offsets _ -> int16 ApiKeys.OffsetRequest
  | RequestTypes.ConsumerMetadata _ -> int16 ApiKeys.ConsumerMetadataRequest
  | RequestTypes.OffsetCommit _ -> int16 ApiKeys.OffsetCommitRequest
  | RequestTypes.OffsetFetch _ -> int16 ApiKeys.OffsetFetchRequest


let transmission_state (result : IAsyncResult) =
  try
    let socket_info = result.AsyncState :?> Socket
    let bytes_sent = socket_info.EndSend(result)
    ("electroelephant.bytes.sent" , float bytes_sent) ||> Log.incrBy logger
    LogLine.Create "successfully sent a message to kafka" |> Log.log logger
  with 
    /// TODO
    /// this needs much more error handling, we need to be able to handle the following:
    /// Leader for partition changed  -> attempt to rebuild metadata
    /// Broker down                   -> attempt to rebuild metadata
    /// General Transmission error    -> do a retry
    | ex -> Log.errorStr "failed when sending data" |> Log.setExn ex |> Log.log logger



let rec receive_response_payload (result : IAsyncResult) =
  try
    let state = result.AsyncState :?> RecieveState
    let bytes_received = end_receive state result

    if bytes_received = 0 then
      //something went horribly wrong
      // TODO: Some sort of error handling
      ()
    else

      // update how far we've read
      state.total_read_bytes <- state.total_read_bytes + bytes_received

      // write the current buffer to the stream.
      state.stream.Write(state.buffer, 0, bytes_received)

      let remaining_data_size = 
        match ( state.msg_size - state.total_read_bytes ) with
        // This is the case when we have looots of data to read still.
        // so lets attempt to fill our buffer.
        | x when x >= state.buffer.Length -> state.buffer.Length

        // this happens when we have a small message or are at the end of the current message
        // so we only read the remaning bytes.
        | x when x < state.buffer.Length -> x

        // This shouldn't happen.
        | _ -> 0 

      begin_receive state remaining_data_size receive_response_payload |> ignore

  with
    | ex -> Log.errorStr "failed while reading data, this needs to be handled." |> Log.log logger


/// this method handles the initial response from the kafka broker. 
/// it grabs the size of the response so we know how much more we need to read.
let receive_response_size (result : IAsyncResult) =
  try
    let state = result.AsyncState :?> RecieveState
    let bytes_received = state.socket.EndReceive(result)

    if bytes_received = 0 then
      //something went horribly wrong
      // TODO: Some sort of error handling
      ()
    else
      // the stream should contain atleast the size and probably some of the response
      // lets find out the size
      let payload_size = BitConverter.ToInt32(state.buffer, 0)
      let payload_state = { state with msg_size = payload_size ; total_read_bytes = 0 }

      /// The error handling is done in the callback.
      state.socket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(receive_response_payload), payload_state) |> ignore
      begin_receive state 

  with
    | ex -> Log.errorStr "failed while reading data, this needs to be handled." |> Log.log logger

/// <summary>
///   Mission: 
///     A. wrap the specific request in to a format which kafka understands
///     B. Set up the correlation id and callback which can be called out-of-band when the response arrives
///     C. Serialize the message
///     D. Figure out which Broker the message should go to
///     E. Transmitt the message to the correct broker
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
let publish_msg<'ResponseType, 'RequestType> (req_t : RequestTypes) (callback : 'ResponseType -> unit) 
    (topic : string) (key : string) (value : 'RequestType) : unit = 

  //B. 
  let current_correlation_id = next_correlation_id ()

  //A. 
  let req = 
    { api_version = 0s
      api_key = api_key req_t
      correlation_id = current_correlation_id
      client_id = get_client_id
      request_type = req_t }
  
  //D. grab the connection to the current broker
  let conn = setup_broker_conn()

  //C. serialize the envelop
  use stream = new MemoryStream()
  ElectroElephant.Request.serialize req stream
  let payload = stream.ToArray()

  //E. transmitt the request
  // lets ignore the async result since that object will be sent to the async callback, we'll handle errors
  // and other stuff there instead.
  conn.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback(transmission_state), conn) |> ignore

  // Start reading the response, we need the first four bytes in order to know how long the response will be.
  let msg_size = Array.zeroCreate 4
  // ignore the result here as well. Let the callback handle errors
  conn.BeginReceive(msg_size,0, msg_size.Length, SocketFlags.None, new AsyncCallback(receive_response_size), conn) |> ignore





//let recieve_loop =
//  let conn = setup_broker_conn()
//
//  while true do
//    if conn.Available > 0 then
//      //http://codebetter.com/gregyoung/2007/06/18/async-sockets-and-buffer-management/
//      //http://en.wikibooks.org/wiki/F_Sharp_Programming/Async_Workflows
//      //http://en.wikibooks.org/wiki/F_Sharp_Programming/MailboxProcessor
//      //http://www.fssnip.net/6c
//      //http://www.fssnip.net/tags/MailboxProcessor
//     ()
//    else
//      Thread.Sleep(100)
//  
//

