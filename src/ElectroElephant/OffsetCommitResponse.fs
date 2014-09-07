module ElectroElephant.OffsetCommitResponse

open ElectroElephant.Common

[<StructuralEquality; StructuralComparison>]
type PartitionCommitResponse = 
  { partition_id : PartitionId
    error_code : ErrorCode }

[<StructuralEquality; StructuralComparison>]
type TopicCommitResponse = 
  { topic_name : TopicName
    partition_commit_response : PartitionCommitResponse list }

[<StructuralEquality; StructuralComparison>]
type OffsetCommitResponse = 
  { topic_commit_response : TopicCommitResponse list }

open ElectroElephant.StreamHelpers
open System.IO

let private serialize_partition_commit (stream : MemoryStream) partition_commit = 
  stream.write_int<PartitionId> partition_commit.partition_id
  stream.write_int<ErrorCode> partition_commit.error_code

let private serialize_topic_commit (stream : MemoryStream) topic_commit = 
  stream.write_str<StringSize> topic_commit.topic_name
  stream.write_int<ArraySize> topic_commit.partition_commit_response.Length
  topic_commit.partition_commit_response |> List.iter (serialize_partition_commit stream)

/// <summary>
///  Serialize a OffsetCommitResponse to the given stream
/// </summary>
/// <param name="stream">the OffsetCommitResponse to serialize</param>
/// <param name="offset_commit">the stream to serialize to</param>
let serialize offset_commit (stream : MemoryStream) = 
  stream.write_int<ArraySize> offset_commit.topic_commit_response.Length
  offset_commit.topic_commit_response |> List.iter (serialize_topic_commit stream)

let private deserialize_partition_commit (stream : MemoryStream) = 
  { partition_id = stream.read_int32<PartitionId>()
    error_code = stream.read_int16<ErrorCode>() }

let private deserialize_topic_commit (stream : MemoryStream) = 
  { topic_name = stream.read_str<StringSize>()
    partition_commit_response = read_array<PartitionCommitResponse> stream deserialize_partition_commit }

/// <summary>
///  Deserializes a OffsetCommitResponse from the given stream
/// </summary>
/// <param name="stream">the stream containing a OffsetCommitResponse</param>
let deserialize (stream : MemoryStream) = 
  { topic_commit_response = read_array<TopicCommitResponse> stream deserialize_topic_commit }
