module ElectroElephant.OffsetFetchRequest

open ElectroElephant.Common

type TopicOffsetFetch =
  { topic_name : TopicName
    partitions : PartitionId list }

type OffsetFetchRequest =
  { consumer_group : ConsumerGroup
    topic_offset_data : TopicOffsetFetch list }

