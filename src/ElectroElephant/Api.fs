﻿module ElectroElephant.Api

open ElectroElephant.Common
open ElectroElephant.Connection
open ElectroElephant.Request
open ElectroElephant.Response
open Microsoft.FSharp.Core.Operators
open System.Collections.Generic

open System.Threading
open System
open System.IO
open System.Net.Sockets

open Logary

let logger = Logging.getCurrentLogger()

type TransitMetadata<'a> = 
  { /// the correlation id, this is how a request is mapped to a response
    correlation_id : int32
    /// Callback that should be called whenever we get a response to our request
    callback : 'a -> unit }

let correlatation_map = 
  new Dictionary<CorrelationId, TransitMetadata<ResponseTypes>>()
let get_client_id = "kamils client"

let next_correlation_id () = 
  1
//  let locket = obj()
//  let mutable corr = ref 0
//  lock locket <| fun _ -> 
//    corr := corr + 1
//    !corr

let api_key (req_t : RequestTypes) = 
  match req_t with
  | RequestTypes.Metadata _ -> int16 ApiKeys.MetadataRequest
  | RequestTypes.Produce _ -> int16 ApiKeys.ProduceRequest
  | RequestTypes.Fetch _ -> int16 ApiKeys.FetchRequest
  | RequestTypes.Offsets _ -> int16 ApiKeys.OffsetRequest
  | RequestTypes.ConsumerMetadata _ -> int16 ApiKeys.ConsumerMetadataRequest
  | RequestTypes.OffsetCommit _ -> int16 ApiKeys.OffsetCommitRequest
  | RequestTypes.OffsetFetch _ -> int16 ApiKeys.OffsetFetchRequest


type TransmissionState =
  { socket : Socket }

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
/// </summary>
/// <param name="req_t">The type of request that we want to transmitt</param>
/// <param name="callback">The callback which should be called when we get a response</param>
/// <param name="topic">which topic the message is for</param>
/// <param name="key">the key decides which partition the message is for</param>
/// <param name="value">what we want to publish</param>
let publish_msg<'ResponseType, 'RequestType> (req_t : RequestTypes) (callback : 'ResponseType -> unit) 
    (topic : string) (key : string) (value : 'RequestType) = 

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
  conn.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback(transmission_state), conn)

let recieve_loop =
  let conn = setup_broker_conn()

  while true do
    if conn.Available > 0 then
      //http://codebetter.com/gregyoung/2007/06/18/async-sockets-and-buffer-management/
      //http://en.wikibooks.org/wiki/F_Sharp_Programming/Async_Workflows
      //http://en.wikibooks.org/wiki/F_Sharp_Programming/MailboxProcessor
      //http://www.fssnip.net/6c
      //http://www.fssnip.net/tags/MailboxProcessor
     ()
    else
      Thread.Sleep(100)
  


