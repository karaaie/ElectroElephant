module ElectroElephant.OffsetFetchRequest

open ElectroElephant.Common
open ElectroElephant.StreamHelpers
open System.IO

[<StructuralEquality; StructuralComparison>]
type TopicOffsetFetch = 
  { topic_name : TopicName
    partitions : PartitionId list }

[<StructuralEquality; StructuralComparison>]
type OffsetFetchRequest = 
  { consumer_group : ConsumerGroup
    topic_offset_data : TopicOffsetFetch list }

let private serialize_topic (stream : MemoryStream) topic_offset = 
  stream.write_str<StringSize> topic_offset.topic_name
  stream.write_int_list<ArraySize, PartitionId> topic_offset.partitions

// Serializes a OffsetFetchRequest to the given stream
let serialize offset_fetch (stream : MemoryStream) = 
  stream.write_str<StringSize> offset_fetch.consumer_group
  stream.write_int<ArraySize> offset_fetch.topic_offset_data.Length
  offset_fetch.topic_offset_data |> List.iter (serialize_topic stream)

let private deserialize_topic_offset (stream : MemoryStream) = 
  { topic_name = stream.read_str<StringSize>()
    partitions = stream.read_int32_list<ArraySize, PartitionId>() }

let private deserialize_topic_offsets (stream : MemoryStream) = 
  let num = stream.read_int32<ArraySize>()
  [ for i in 1..num do
      yield deserialize_topic_offset stream ]

/// Deserializes a OffsetFetchRequest from the given stream
let deserialize (stream : MemoryStream) = 
  { consumer_group = stream.read_str<StringSize>()
    topic_offset_data = deserialize_topic_offsets stream }
