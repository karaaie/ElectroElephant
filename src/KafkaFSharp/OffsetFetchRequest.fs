module ElectroElephant.OffsetFetchRequest

open ElectroElephant.Common

[<StructuralEquality;StructuralComparison>]
type TopicOffsetFetch =
  { topic_name : TopicName
    partitions : PartitionId list }

[<StructuralEquality;StructuralComparison>]
type OffsetFetchRequest =
  { consumer_group : ConsumerGroup
    topic_offset_data : TopicOffsetFetch list }

