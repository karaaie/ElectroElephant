module ElectroElephant.MetadataResponse

open ElectroElephant.Common

type Broker =
  { node_id : NodeId
    host    : Hostname
    port    : Port }

type PartitionMetadata = 
  { error_code : ErrorCode

    id : PartitionId

    // The node id for the kafka broker currently acting as leader for this 
    // partition. If no leader exists because we are in 
    // the middle of a leader election this id will be -1.
    leader : LeaderId

    // The set of alive nodes that currently acts as slaves for the leader for this partition.
    replicas : ReplicaId list

    //The set subset of the replicas that are "caught up" to the leader
    isr : Isr list }

type TopicMetadata =
  { error_code  : ErrorCode
    name        : TopicName
    partitions  : PartitionMetadata list } 

type MetadataResponse =
  { brokers         : Broker list
    topic_metadatas : TopicMetadata list }

