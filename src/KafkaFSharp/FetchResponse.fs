module ElectroElephant.FetchResponse

open ElectroElephant.Common
open ElectroElephant.Message

[<StructuralEquality;StructuralComparison>]
type FetchedPartitionMessage =
    // The id of the partition this response is for.
  { partition_id : PartitionId
    error_code : ErrorCode
    // The offset at the end of the log for this partition. This 
    // can be used by the client to determine how many 
    // messages behind the end of the log they are.
    highwater_mark_offset : Offset
    // The size in bytes of the message set for this partition
    message_set_size : MessageSize 
    // The message data fetched from this partition, in the format described above.
    message_set : MessageSet }

[<StructuralEquality;StructuralComparison>]
type FetchedTopicMessage =
  // The name of the topic this response entry is for.
  { topic_name : TopicName
    partition_messages : FetchedPartitionMessage list}

[<StructuralEquality;StructuralComparison>]
type FetchResponse = 
  { topic_messages : FetchedTopicMessage list }


open System.IO
open ElectroElephant.StreamHelpers

let private serialize_partition_message (stream : MemoryStream) partition_message =
  stream.write_int<PartitionId> partition_message.partition_id
  stream.write_int<ErrorCode> partition_message.error_code
  stream.write_int<Offset> partition_message.highwater_mark_offset
  stream.write_int<MessageSize> partition_message.message_set_size
  Message.serialize partition_message.message_set stream

let private serialize_topic_messages (stream : MemoryStream) fetch_topic =
  stream.write_str<StringSize> fetch_topic.topic_name
  stream.write_int<ArraySize> fetch_topic.partition_messages.Length
  fetch_topic.partition_messages |> List.iter (serialize_partition_message stream)

/// <summary>
///  Writes the FetchResponse to the provided stream
/// </summary>
/// <param name="fetch_resp">FetchResponse to be serialized</param>
/// <param name="stream">stream to write serialization to</param>
let serialize fetch_resp (stream : MemoryStream) =
  stream.write_int<ArraySize> fetch_resp.topic_messages.Length
  fetch_resp.topic_messages |> List.iter (serialize_topic_messages stream)

let private deserialize_partition_response (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId> ()
    error_code = stream.read_int16<ErrorCode> ()
    highwater_mark_offset = stream.read_int64<Offset> ()
    message_set_size = stream.read_int32<MessageSize>()
    message_set = Message.deserialize stream}

let private deserialize_partition_messages (stream : MemoryStream ) =
  let num_partitions = stream.read_int32<ArraySize>()
  [for i in 1..num_partitions do yield deserialize_partition_response stream]

let private deserialize_topic_responses (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> () 
    partition_messages = deserialize_partition_messages stream}

/// <summary>
///  Reads a FetchResponse from the provided stream
/// </summary>
/// <param name="stream">stream which contains a FetchResponse</param>
let deserialize (stream : MemoryStream) : FetchResponse =
  let num_topics = stream.read_int32<ArraySize>()
  {topic_messages = [for i in 1..num_topics do yield deserialize_topic_responses stream]}
