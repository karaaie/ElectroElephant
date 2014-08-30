module ElectroElephant.FetchResponse

open ElectroElephant.Common
open ElectroElephant.Message

type FetchedPartitionMessage =
    // The id of the partition this response is for.
  { partition_id : PartitionId
    error_code : ErrorCode
    // The offset at the end of the log for this partition. This 
    // can be used by the client to determine how many 
    // messages behind the end of the log they are.
    highwater_mark_offset : Offset
    // The size in bytes of the message set for this partition
    message_set_size : MessageSize 
    // The message data fetched from this partition, in the format described above.
    message_set : MessageSet }

type FetchedTopicMessage =
  // The name of the topic this response entry is for.
  { topic_name : TopicName
    partition_messages : FetchedPartitionMessage list}

type FetchResponse = 
  { topic_messages : FetchedTopicMessage list }

