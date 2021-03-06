﻿module ElectroElephant.ProduceResponse

open ElectroElephant.Common
open ElectroElephant.StreamHelpers
open System.IO

[<StructuralEquality; StructuralComparison>]
type ProducePartitionResponse = 
  { /// The partition this response entry corresponds to.
    partition_id : PartitionId
    /// The error from this partition, if any. Errors are given on a per-partition 
    /// basis because a given partition may be unavailable or maintained 
    /// on a different host, while others may have successfully 
    /// accepted the produce request.
    error_code : ErrorCode
    /// The offset assigned to the first message in the message set appended to this partition.
    offset : Offset }

[<StructuralEquality; StructuralComparison>]
type ProduceTopicResponse = 
  { /// The topic this response entry corresponds to.
    topic_name : TopicName
    partition_responses : ProducePartitionResponse list }

[<StructuralEquality; StructuralComparison>]
type ProduceResponse = 
  { topic_responses : ProduceTopicResponse list }

let private serialize_partition_response (stream : MemoryStream) partition_response = 
  stream.write_int<PartitionId> partition_response.partition_id
  stream.write_int<ErrorCode> partition_response.error_code
  stream.write_int<Offset> partition_response.offset

let private serialize_topic_response (stream : MemoryStream) topic_response = 
  stream.write_str<StringSize> topic_response.topic_name
  stream.write_int<ArraySize> topic_response.partition_responses.Length
  topic_response.partition_responses |> List.iter (serialize_partition_response stream)

/// <summary>
///   Serialize a ProduceResponse to a bytestream
/// </summary>
/// <param name="prod_resp">the produceresponse to serialize</param>
/// <param name="stream">bytestream to write serialization to</param>
let serialize prod_resp (stream : MemoryStream) = 
  stream.write_int<ArraySize> prod_resp.topic_responses.Length
  prod_resp.topic_responses |> List.iter (serialize_topic_response stream)

let private deserialize_partition_response (stream : MemoryStream) = 
  { partition_id = stream.read_int32<PartitionId>()
    error_code = stream.read_int16<ErrorCode>()
    offset = stream.read_int64<Offset>() }

let private deserialize_topic_response (stream : MemoryStream) = 
  { topic_name = stream.read_str<StringSize>()
    partition_responses = read_array<ProducePartitionResponse> stream deserialize_partition_response }

/// <summary>
///  Deserialize a ProduceResponse from the bytestream
/// </summary>
/// <param name="stream">the bytestream which contains the ProduceResponse</param>
let deserialize (stream : MemoryStream) = 
  { topic_responses = read_array<ProduceTopicResponse> stream deserialize_topic_response }