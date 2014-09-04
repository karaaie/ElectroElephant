module ElectroElephant.ConsumerMetadataResponse

open System.IO
open ElectroElephant.Common
open ElectroElephant.StreamHelpers


type ConsumerMetadataResponse =
  { error_code        : ErrorCode 
    coordinator_id    : CoordinatorId
    coordinator_host  : Hostname
    coordinator_port  : Port}

/// <summary>
///  Serializes a ConsumerMetadataResponse to the given stream
/// </summary>
/// <param name="consumer_metadata">the ConsumerMetadataResponse to serialize</param>
/// <param name="stream"> the stream to serialize to </param>
let serialize consumer_metadata (stream : MemoryStream) = 
  stream.write_int<ErrorCode> consumer_metadata.error_code
  stream.write_int<CoordinatorId> consumer_metadata.coordinator_id
  stream.write_str<StringSize> consumer_metadata.coordinator_host
  stream.write_int<Port> consumer_metadata.coordinator_port

/// <summary>
///  Deserialize a ConsumerMetadataResponse from the given stream
/// </summary>
/// <param name="stream">stream containing ConsumerMetadataResponse</param>
let deserialize (stream : MemoryStream) =
  { error_code = stream.read_int16<ErrorCode> ()
    coordinator_id = stream.read_int32<CoordinatorId> ()
    coordinator_host = stream.read_str<Hostname> ()
    coordinator_port = stream.read_int32<Port> ()}
