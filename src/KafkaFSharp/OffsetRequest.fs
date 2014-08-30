module ElectroElephant.OffsetRequest

open ElectroElephant.Common

type PartitionFetchRequestData =
  { partition_id : PartitionId
    // Used to ask for all messages before a certain time (ms). There 
    // are two special values. 
    // Specify: 
    //  -1 to receive the latest offset (i.e. the 
    //     offset of the next coming message) and 
    //  -2 to receive the earliest available offset. 
    //     Note that because offsets are pulled in descending order, 
    //     asking for the earliest offset will always return you 
    //     a single element.
    time : Time 
    max_number_of_offsets : MaxNumberOfOffsets }

type TopicOffsetRequestData =
  { topic_name : TopicName
    partition_fetch_data : PartitionFetchRequestData list }

type OffsetRequest = 
  { replica_id : ReplicaId
    topic_offset_data : TopicOffsetRequestData list}
