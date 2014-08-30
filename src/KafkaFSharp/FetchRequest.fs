module ElectroElephant.FetchRequest

open ElectroElephant.Common

type FetchPartitionData =
     // The id of the partition the fetch is for.
    { partition_id : PartitionId
     // The offset to begin this fetch from.
      fetch_offset : Offset
     // The maximum bytes to include in the message set for this partition. This helps bound the size of the response.
      max_bytes : MaxBytes }

type FetchTopicData =
     // The name of the topic.
    { topic_name : TopicName
      partition_data : FetchPartitionData list }

type FetchRequest =
     // The replica id indicates the node id of the replica initiating this 
     // request. Normal client consumers should always specify 
     // this as -1 as they have no node id. Other brokers set 
     // this to be their own node id. The value -2 is accepted 
     // to allow a non-broker to issue fetch requests as if it
     // were a replica broker for debugging purposes.
    { replica_id : ReplicaId

     // The max wait time is the maximum amount of time 
     // in milliseconds to block waiting if insufficient 
     // data is available at the time the request is issued.
      max_wait_time : MaxWaitTime

     // This is the minimum number of bytes of messages that must be available to 
     // give a response. If the client sets this to 0 the server will 
     // always respond immediately, however if there is no new data 
     // since their last request they will just get back 
     // empty message sets. If this is set to 1, the server 
     // will respond as soon as at least one partition has at 
     // least 1 byte of data or the specified timeout occurs. 
     // By setting higher values in combination with the timeout the consumer 
     // can tune for throughput and trade a little additional latency 
     // for reading only large chunks of data (e.g. setting 
     // MaxWaitTime to 100 ms and setting MinBytes to 64k would 
     // allow the server to wait up to 100ms to try to accumulate 64k of data before responding).
      min_bytes : MinBytes
      topic_data : FetchTopicData list }
