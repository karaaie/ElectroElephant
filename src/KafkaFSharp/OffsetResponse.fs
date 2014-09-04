﻿module ElectroElephant.OffsetResponse

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

open System.IO
open ElectroElephant.StreamHelpers

let private serialize_partition_offset (stream : MemoryStream) partition_off =
  stream.write_int<PartitionId> partition_off.partition_id
  stream.write_int<ErrorCode> partition_off.error_code
  stream.write_int_list<ArraySize, Offset> partition_off.offsets

let private serialize_topic_offset (stream : MemoryStream) topic_off =
  stream.write_str<StringSize> topic_off.topic_name
  stream.write_int<ArraySize> topic_off.partition_offset_data.Length
  topic_off.partition_offset_data |> List.iter (serialize_partition_offset stream)

/// <summary>
///  Serializes a OffsetResponse to the given stream
/// </summary>
/// <param name="off_resp">OffsetPartition to be serialized</param>
/// <param name="stream">stream to serialize to</param>
let serialize off_resp (stream : MemoryStream ) =
  stream.write_int<ArraySize> off_resp.topic_offset_data.Length
  off_resp.topic_offset_data |> List.iter (serialize_topic_offset stream)

let private deserialize_partition_offset (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId> ()
    error_code = stream.read_int16<ErrorCode> ()
    offsets = stream.read_int64_list<ArraySize,Offset> ()}

let private deserialize_partition_offsets (stream : MemoryStream) =
  let num_partitions = stream.read_int32<ArraySize> ()
  [for i in 1..num_partitions do yield deserialize_partition_offset stream]

let private  deserialize_topic_offsets (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> ()
    partition_offset_data = deserialize_partition_offsets stream}
 
/// <summary>
///  Deserializes  a OffsetResponse from the given stream
/// </summary>
/// <param name="stream">stream containing a OffsetResponse</param>
let deserialize (stream : MemoryStream) =
  let num_topics = stream.read_int32<ArraySize>()
  { topic_offset_data =
    [for i in 1..num_topics do yield deserialize_topic_offsets stream]}
