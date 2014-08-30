module ElectroElephant.OffsetResponse

open ElectroElephant.Common

type PartitionOffsetResponseData =
  { partition_id : PartitionId 
    error_code : ErrorCode
    offsets : Offset list}

type TopicOffsetResponseData =
  { topic_name : TopicName
    partition_offset_data : PartitionOffsetResponseData list}

type OffsetResponse =
  { topic_offset_data : TopicOffsetResponseData list}

