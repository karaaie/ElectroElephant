module ElectroElephant.ProduceResponse

open ElectroElephant.Common

type ProducePartitionResponse =
    // The partition this response entry corresponds to.
    { partition_id : PartitionId
    // The error from this partition, if any. Errors are given on a per-partition 
    // basis because a given partition may be unavailable or maintained 
    // on a different host, while others may have successfully 
    // accepted the produce request.
      error_code : ErrorCode
    // The offset assigned to the first message in the message set appended to this partition.
      offset : Offset }

type ProduceTopicResponse =
    // The topic this response entry corresponds to.
    { topic_name : TopicName 
      partition_responses : ProducePartitionResponse list}

type ProduceResponse =
    { topic_responses : ProduceTopicResponse list }

