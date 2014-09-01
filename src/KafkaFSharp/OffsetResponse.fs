module ElectroElephant.OffsetResponse

open System.IO

open ElectroElephant.Common
open ElectroElephant.StreamHelpers

[<StructuralEquality;StructuralComparison>]
type PartitionOffsetResponseData =
  { partition_id : PartitionId 
    error_code : ErrorCode
    offsets : Offset list}

[<StructuralEquality;StructuralComparison>]
type TopicOffsetResponseData =
  { topic_name : TopicName
    partition_offset_data : PartitionOffsetResponseData list}

[<StructuralEquality;StructuralComparison>]
type OffsetResponse =
  { topic_offset_data : TopicOffsetResponseData list}

let private serialize_partition_offset (stream : MemoryStream) partition_offset =
  stream.write_int<PartitionId> partition_offset.partition_id
  stream.write_int<ErrorCode> partition_offset.error_code
  stream.write_int_list<ArraySize, Offset> partition_offset.offsets

let private serialize_topic_offset (stream : MemoryStream) topic_offset =
  stream.write_str<StringSize> topic_offset.topic_name
  stream.write_int<ArraySize> topic_offset.partition_offset_data.Length
  topic_offset.partition_offset_data |> List.iter (serialize_partition_offset stream)

/// <summary>
///  Serializes OffsetResponse to a stream
/// </summary>
/// <param name="partition_offset_resp">the OffsetRespons to serialize</param>
/// <param name="stream">the stream to serialize to</param>
let serialize offset_resp (stream : MemoryStream) =
  stream.write_int<ArraySize> offset_resp.topic_offset_data.Length
  offset_resp.topic_offset_data |> List.iter (serialize_topic_offset stream)

let private deserialize_partition_offset_data (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId> ()
    error_code = stream.read_int16<ErrorCode> ()
    offsets = stream.read_int64_list<ArraySize,Offset> ()}

let private deserialize_partition_offset_datas (stream : MemoryStream) =
  let num_parts = stream.read_int32<ArraySize>()
  [for i in 1..num_parts do yield deserialize_partition_offset_data stream]

let private deserialize_topic_data (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> ()
    partition_offset_data = deserialize_partition_offset_datas stream}

/// <summary>
///   Deserialize a OffsetResponse from the stream
/// </summary>
/// <param name="stream">a stream which contains a OffsetResponse</param>
let deserialize (stream : MemoryStream) =
  let num_topics = stream.read_int32<ArraySize> ()
  { topic_offset_data = [for i in 1..num_topics do yield deserialize_topic_data stream]}