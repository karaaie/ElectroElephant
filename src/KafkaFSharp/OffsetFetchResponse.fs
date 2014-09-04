module ElectroElephant.OffsetFetchResponse

open System.IO
open ElectroElephant.StreamHelpers
open ElectroElephant.Common

type PartitionFetchResponseData =
  { partition_id : PartitionId
    offset : Offset
    metadata : Metadata
    error_code : ErrorCode}

type TopicFetchResponseData =
  { topic_name : TopicName
    partition_fetch_data : PartitionFetchResponseData list}

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
  { partition_id = stream.read_int32<PartitionId> ()
    offset = stream.read_int64<Offset> ()
    metadata = stream.read_str<StringSize> ()
    error_code = stream.read_int16<ErrorCode> ()}

let private deserialize_partition_offsets (stream : MemoryStream) =
  let num_par = stream.read_int32<ArraySize> ()
  [for i in 1..num_par do yield deserialize_partition_offset stream]

let private deserialize_topic_offset (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> ()
    partition_fetch_data = deserialize_partition_offsets stream}

/// <summary>
///  Deserialize a OffsetFetchResponse from the given stream
/// </summary>
/// <param name="stream">the stream containing a OffsetFetchResponse</param>
let deserialize (stream : MemoryStream) =
  let num_topics = stream.read_int32<ArraySize> ()
  {topic_offset_data = [for i in 1..num_topics do yield deserialize_topic_offset stream]}