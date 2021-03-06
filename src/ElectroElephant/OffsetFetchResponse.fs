﻿module ElectroElephant.OffsetFetchResponse

open ElectroElephant.Common
open ElectroElephant.StreamHelpers
open System.IO

[<StructuralEquality; StructuralComparison>]
type PartitionFetchResponseData = 
  { partition_id : PartitionId
    offset : Offset
    metadata : Metadata
    error_code : ErrorCode }

[<StructuralEquality; StructuralComparison>]
type TopicFetchResponseData = 
  { topic_name : TopicName
    partition_fetch_data : PartitionFetchResponseData list }

[<StructuralEquality; StructuralComparison>]
type OffsetFetchResponse = 
  { topic_offset_data : TopicFetchResponseData list }

let private serialize_partition_offset (stream : MemoryStream) partition_off = 
  stream.write_int<PartitionId> partition_off.partition_id
  stream.write_int<Offset> partition_off.offset
  stream.write_str<StringSize> partition_off.metadata
  stream.write_int<ErrorCode> partition_off.error_code

let private serialize_topic_offset_data (stream : MemoryStream) topic_off = 
  stream.write_str<StringSize> topic_off.topic_name
  stream.write_int<ArraySize> topic_off.partition_fetch_data.Length
  topic_off.partition_fetch_data |> List.iter (serialize_partition_offset stream)

/// <summary>
///   Serializes a OffsetFetchResponse to the given stream
/// </summary>
/// <param name="offset_fetch">the OffsetFetchResponse to serialize</param>
/// <param name="stream">the stream to serialize to</param>
let serialize offset_fetch (stream : MemoryStream) = 
  stream.write_int<ArraySize> offset_fetch.topic_offset_data.Length
  offset_fetch.topic_offset_data |> List.iter (serialize_topic_offset_data stream)

let private deserialize_partition_offset (stream : MemoryStream) = 
  { partition_id = stream.read_int32<PartitionId>()
    offset = stream.read_int64<Offset>()
    metadata = stream.read_str<StringSize>()
    error_code = stream.read_int16<ErrorCode>() }

let private deserialize_topic_offset (stream : MemoryStream) = 
  { topic_name = stream.read_str<StringSize>()
    partition_fetch_data = read_array<PartitionFetchResponseData> stream deserialize_partition_offset }

/// <summary>
///  Deserialize a OffsetFetchResponse from the given stream
/// </summary>
/// <param name="stream">the stream containing a OffsetFetchResponse</param>
let deserialize (stream : MemoryStream) = 
  { topic_offset_data = read_array<TopicFetchResponseData> stream deserialize_topic_offset }