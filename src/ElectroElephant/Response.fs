module ElectroElephant.Response

open ElectroElephant.Common
open ElectroElephant.ConsumerMetadataResponse
open ElectroElephant.FetchResponse
open ElectroElephant.MetadataResponse
open ElectroElephant.OffsetCommitResponse
open ElectroElephant.OffsetFetchResponse
open ElectroElephant.OffsetResponse
open ElectroElephant.ProduceResponse
open ElectroElephant.StreamHelpers
open System.IO

type ResponseTypes = 
  | Metadata of MetadataResponse
  | Produce of ProduceResponse
  | Fetch of FetchResponse
  | Offset of OffsetResponse
  | ConsumerMetadata of ConsumerMetadataResponse
  | CommitOffset of OffsetCommitResponse
  | FetchOffset of OffsetFetchResponse

type Response = 
  { // The server passes back whatever integer the client supplied as 
    // the correlation in the request.
    correlation_id : CorrelationId
    response_type : ResponseTypes }

let serialize resp (stream : MemoryStream) = 
  stream.write_int<CorrelationId> resp.correlation_id
  match resp.response_type with
  | ResponseTypes.Metadata m -> 
    ElectroElephant.MetadataResponse.serialize m stream
  | ResponseTypes.Produce p -> 
    ElectroElephant.ProduceResponse.serialize p stream
  | ResponseTypes.Fetch f -> ElectroElephant.FetchResponse.serialize f stream
  | ResponseTypes.Offset o -> ElectroElephant.OffsetResponse.serialize o stream
  | ResponseTypes.ConsumerMetadata cm -> 
    ElectroElephant.ConsumerMetadataResponse.serialize cm stream
  | ResponseTypes.CommitOffset co -> 
    ElectroElephant.OffsetCommitResponse.serialize co stream
  | ResponseTypes.FetchOffset fo -> 
    ElectroElephant.OffsetFetchResponse.serialize fo stream
