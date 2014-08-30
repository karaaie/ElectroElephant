module ElectroElephant.OffsetCommitResponse

open ElectroElephant.Common

type PartitionCommitResponse =
  { partition_id : PartitionId
    error_code : ErrorCode }

type TopicCommitResponse =
  { topic_name : TopicName
    partition_commit_response : PartitionCommitResponse list }

type OffsetCommitResponse =
  { topic_commit_response : TopicCommitResponse list}