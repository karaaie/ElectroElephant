module ElectroElephant.Api

open ElectroElephant.Common
open ElectroElephant.Connection
open ElectroElephant.Request
open ElectroElephant.Response
open Microsoft.FSharp.Core.Operators
open System.Collections.Generic
open System.IO
open System.Net.Sockets

type TransitMetadata<'a> = 
  { /// the correlation id, this is how a request is mapped to a response
    correlation_id : int32
    /// Callback that should be called whenever we get a response to our request
    callback : 'a -> unit }

let correlatation_map = 
  new Dictionary<CorrelationId, TransitMetadata<ResponseTypes>>()
let get_client_id = "kamils client"

let next_correlation_id = 
  let locket = obj()
  let mutable corr = ref 0
  lock locket <| fun _ -> 
    corr := corr + 1
    !corr

let api_key (req_t : RequestTypes) = 
  match req_t with
  | RequestTypes.Metadata _ -> int16 ApiKeys.MetadataRequest
  | RequestTypes.Produce _ -> int16 ApiKeys.ProduceRequest
  | RequestTypes.Fetch _ -> int16 ApiKeys.FetchRequest
  | RequestTypes.Offsets _ -> int16 ApiKeys.OffsetRequest
  | RequestTypes.ConsumerMetadata _ -> int16 ApiKeys.ConsumerMetadataRequest
  | RequestTypes.OffsetCommit _ -> int16 ApiKeys.OffsetCommitRequest
  | RequestTypes.OffsetFetch _ -> int16 ApiKeys.OffsetFetchRequest

let publish_msg<'a> (req_t : RequestTypes) (callback : 'a -> unit) 
    (topic : string) (key : string) (value : 'a) = 
  let req = 
    { api_version = 0s
      api_key = api_key req_t
      correlation_id = next_correlation_id
      client_id = get_client_id
      request_type = req_t }
  
  let conn = setup_broker_conn()
  use stream = new MemoryStream()
  ElectroElephant.Request.serialize req stream
  let payload = stream.ToArray()
  conn.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null)
