module ElectroElephant.ProduceResponse

open System.IO

open ElectroElephant.StreamHelpers
open ElectroElephant.Common


type ProducePartitionResponse =
    // The partition this response entry corresponds to.
    { partition_id : PartitionId
    // The error from this partition, if any. Errors are given on a per-partition 
    // basis because a given partition may be unavailable or maintained 
    // on a different host, while others may have successfully 
    // accepted the produce request.
      error_code : ErrorCode
    // The offset assigned to the first message in the message set appended to this partition.
      offset : Offset }

type ProduceTopicResponse =
    // The topic this response entry corresponds to.
    { topic_name : TopicName 
      partition_responses : ProducePartitionResponse list}

type ProduceResponse =
    { topic_responses : ProduceTopicResponse list }

let private serialize_partition_response (stream : MemoryStream) partition_response =
  stream.write_int<PartitionId> partition_response.partition_id
  stream.write_int<ErrorCode> partition_response.error_code
  stream.write_int<Offset> partition_response.offset

let private serialize_topic_response (stream : MemoryStream) topic_response =
  stream.write_str<StringSize> topic_response.topic_name
  stream.write_int<ArraySize> topic_response.partition_responses.Length
  topic_response.partition_responses 
    |> List.iter (serialize_partition_response stream)

/// <summary>
///   Serialize a ProduceResponse to a bytestream
/// </summary>
/// <param name="prod_resp">the produceresponse to serialize</param>
/// <param name="stream">bytestream to write serialization to</param>
let serialize prod_resp (stream : MemoryStream) =
  stream.write_int<ArraySize> prod_resp.topic_responses.Length
  prod_resp.topic_responses |> List.iter (serialize_topic_response stream)

let private deserialize_partition_response (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId> ()
    error_code = stream.read_int16<ErrorCode> ()
    offset = stream.read_int64<Offset> () }

let private deserialize_partition_responses (stream : MemoryStream) =
  let num_partitions = stream.read_int32<ArraySize> ()
  [for i in 1..num_partitions do yield deserialize_partition_response stream]

let private deserialize_topic_response (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> ()
    partition_responses = deserialize_partition_responses stream}

let private deserialize_topic_responses (stream : MemoryStream) =
  let num_topics = stream.read_int32<ArraySize> ()
  [for i in 1..num_topics do yield deserialize_topic_response stream]

/// <summary>
///  Deserialize a ProduceResponse from the bytestream
/// </summary>
/// <param name="stream">the bytestream which contains the ProduceResponse</param>
let deserialize (stream : MemoryStream) =
  {topic_responses = deserialize_topic_responses stream}