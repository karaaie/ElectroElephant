module ElectroElephant.FetchResponse

open System.IO
open ElectroElephant.Common
open ElectroElephant.Message
open ElectroElephant.StreamHelpers

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

let private serialize_partition_message (stream : MemoryStream) partition_message =
  stream.write_int<PartitionId> partition_message.partition_id
  stream.write_int<ErrorCode> partition_message.error_code
  stream.write_int<Offset> partition_message.highwater_mark_offset
  stream.write_int<MessageSize> partition_message.message_set_size
  Message.serialize partition_message.message_set stream

let private serialize_topic_message (stream : MemoryStream) topic_message =
  stream.write_str<StringSize> topic_message.topic_name
  stream.write_int<ArraySize> topic_message.partition_messages.Length
  topic_message.partition_messages 
    |> List.iter (serialize_partition_message stream) 

/// <summary>
///  serialize fetchresponse to the given stream
/// </summary>
/// <param name="fetch">fetchresponse to serialize</param>
/// <param name="stream">stream to serialize to</param>
let serialize fetch (stream : MemoryStream) =
  stream.write_int<ArraySize> fetch.topic_messages.Length
  fetch.topic_messages |> List.iter (serialize_topic_message stream)

let private deserialize_partition_message (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId> ()
    error_code = stream.read_int16<ErrorCode> ()
    highwater_mark_offset = stream.read_int64<Offset> ()
    message_set_size = stream.read_int32<MessageSize> ()
    message_set = Message.deserialize stream }

let private deserialize_partition_messages (stream : MemoryStream) =
  let num_partitions = stream.read_int32<ArraySize>()
  [for i in 1..num_partitions do yield deserialize_partition_message stream]

let private deserialize_topic_message (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize>()
    partition_messages = deserialize_partition_messages stream}

/// <summary>
///   deserializes a FetchResponse from the given stream
/// </summary>
/// <param name="stream">stream containing FetchResponse</param>
let deserialize (stream : MemoryStream) = 
  let num_topics = stream.read_int32<ArraySize>()
  { topic_messages = [for i in 1..num_topics do yield deserialize_topic_message stream]}
