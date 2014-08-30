module ElectroElephant.OffsetFetchResponse

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