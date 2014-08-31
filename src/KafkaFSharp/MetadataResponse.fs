module ElectroElephant.MetadataResponse

open System.IO

open ElectroElephant.Common
open ElectroElephant.StreamHelpers

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

let private serialize_broker (stream : MemoryStream) broker =
  stream.write_int<NodeId>(broker.node_id)
  stream.write_str<StringSize>(broker.host)
  stream.write_int<Port>(broker.port)

let private serialize_partition
      (stream : MemoryStream)
      (partition : PartitionMetadata) =

  stream.write_int<ErrorCode> partition.error_code
  stream.write_int<LeaderId> partition.leader
  stream.write_int_list<ByteArraySize, ReplicaId> partition.replicas
  stream.write_int_list<ByteArraySize, Isr> partition.isr

let private serialize_topic (stream : MemoryStream) topic =
  stream.write_int<ErrorCode> topic.error_code
  stream.write_str<StringSize> topic.name
  topic.partitions |> List.iter (serialize_partition stream)

let serialize meta_resp (stream : MemoryStream) =
  meta_resp.brokers |> List.iter (serialize_broker stream)
  meta_resp.topic_metadatas |> List.iter (serialize_topic stream)

let deserialize (stream : MemoryStream) : MetadataResponse =
  ()