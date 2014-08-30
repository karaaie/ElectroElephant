module ElectroElephant.Response

open ElectroElephant.Common

open ElectroElephant.MetadataResponse
open ElectroElephant.ProduceResponse
open ElectroElephant.FetchResponse
open ElectroElephant.OffsetResponse
open ElectroElephant.ConsumerMetadataResponse
open ElectroElephant.OffsetFetchResponse
open ElectroElephant.OffsetCommitResponse

type ResponseTypes =
  | Metadata of MetadataResponse
  | Produce of ProduceResponse
  | Fetch of FetchResponse
  | Offset of OffsetResponse
  | ConsumerMetadata of ConsumerMetadataResponse
  | CommitOffset of OffsetCommitResponse
  | FetchOffset of OffsetFetchResponse

type Response =
  // The server passes back whatever integer the client supplied as 
  // the correlation in the request.
  { correlation_id : CorrelationId
    response_type : ResponseTypes }

