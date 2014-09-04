module ElectroElephant.OffsetCommitRequest

open ElectroElephant.Common

[<StructuralEquality;StructuralComparison>]
type PartitionOffsetCommit =
  { partition_id : PartitionId
    partition_offset : Offset
    // If the time stamp field is set to -1, then the broker sets the 
    // time stamp to the receive time before committing the offset.
    timestamp : Time
    metadata : Metadata }

[<StructuralEquality;StructuralComparison>]
type TopicOffsetCommit =
  { topic_name : TopicName
    partition_offset_commit : PartitionOffsetCommit list}

[<StructuralEquality;StructuralComparison>]
type OffsetCommitRequest = 
  { consumer_group : ConsumerGroup
    topic_offset_commit : TopicOffsetCommit list }