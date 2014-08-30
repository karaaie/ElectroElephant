module ElectroElephant.Request

open ElectroElephant.Common

open ElectroElephant.MetadataRequest
open ElectroElephant.ProduceRequest
open ElectroElephant.FetchRequest
open ElectroElephant.OffsetRequest
open ElectroElephant.ConsumerMetadataRequest
open ElectroElephant.OffsetCommitRequest
open ElectroElephant.OffsetFetchRequest

type RequestTypes =
  // Describes the currently available brokers, their host and port information,
  // and gives information about which broker hosts which partitions.
  | Metadata of MetadataRequest
  // Send messages to a broker
  | Send of ProduceRequest
  // Fetch messages from a broker, one which fetches data, one which
  // gets cluster metadata, and one which gets offset information about a topic.
  | Fetch of FetchRequest
  // Get information about the available offsets for a given topic partition.
  | Offsets of OffsetRequest
  // Gets the metadata for the commit broker.
  | ConsumerMetadata of ConsumerMetadataRequest
  // Commit a set of offsets for a consumer group
  | OffsetCommit of OffsetCommitRequest
  // Fetch a set of offsets for a consumer group
  | OffsetFetch  of OffsetFetchRequest

type Request =
  // This is a numeric id for the API being invoked (i.e. is it a 
  // metadata request, a produce request, a fetch request, etc).
  { api_key : ApiKey

  // This is a numeric version number for this api. We version each 
  // API and this version number allows the server to properly interpret
  // the request as the protocol evolves. Responses will always be in 
  // the format corresponding to the request version. 
  // Currently the supported version for all APIs is 0.
    api_version : ApiVersion

  // This is a user-supplied integer. It will be passed back in the response
  // by the server, unmodified. It is useful for matching request and 
  // response between the client and server.
    correlation_id : CorrelationId

  // This is a user supplied identifier for the client application. The user 
  // can use any identifier they like and it will be used when logging
  // errors, monitoring aggregates, etc. For example, one might want to 
  // monitor not just the requests per second overall, but the number 
  // coming from each client application (each of which could reside on 
  // multiple servers). This id acts as a logical grouping across all requests from a particular client.
    client_id : ClientId

  // which API this request targets.
    request_type : RequestTypes }