module ElectroElephant.ProduceRequest

open ElectroElephant.Common
open ElectroElephant.Message

[<StructuralEquality;StructuralComparison>]
type PartitionData =
    // The partition that data is being published to.
  { partition_id : PartitionId
    // The size, in bytes, of the message set that follows.
    message_set_size : MessageSetSize
    // A set of messages in the standard format described above.
    message_set : MessageSet }

[<StructuralEquality;StructuralComparison>]
type TopicData =
    // The topic that data is being published to.
  { topic_name : TopicName
    partition_data : PartitionData list }

[<StructuralEquality;StructuralComparison>]
type ProduceRequest =
    // This field indicates how many acknowledgements the servers should receive 
    // before responding to the request. If it is 0 the server will not send any 
    // response (this is the only case where the server will not reply 
    // to a request). If it is 1, the server will wait the data is 
    // written to the local log before sending a response. If it is 
    // -1 the server will block until the message is committed by all 
    // in sync replicas before sending a response. For any number > 1 the server 
    // will block waiting for this number of acknowledgements to occur (but 
    // the server will never wait for more acknowledgements 
    // than there are in-sync replicas).
  { required_acks : RequiredAcks
    // This provides a maximum time in milliseconds the server can await 
    // the receipt of the number of acknowledgements in RequiredAcks. The timeout is 
    // not an exact limit on the request time for a few reasons: 
    // (1) it does not include network latency, 
    // (2) the timer begins at the beginning of the processing of 
    //     this request so if many requests are queued due to server 
    //     overload that wait time will not be included, 
    // (3) we will not terminate a local write so if the local write 
    //     time exceeds this timeout it will not be respected. To get a 
    //     hard timeout of this type the client should use the socket timeout.
    timeout : Timeout
    topic_data : TopicData list }

open System.IO
open ElectroElephant.StreamHelpers

let private serialize_partition_data (stream : MemoryStream) partition =
  stream.write_int<PartitionId> partition.partition_id
  stream.write_int<MessageSetSize> partition.message_set_size
  Message.serialize partition.message_set stream

let private serialize_topic_data (stream : MemoryStream) topic =
  stream.write_str<StringSize> topic.topic_name
  stream.write_int<ArraySize> topic.partition_data.Length
  topic.partition_data |> List.iter (serialize_partition_data stream)

/// <summary>
///  Serializes a ProduceRequest to a bytestream
/// </summary>
/// <param name="prod_req">the produce request to be serialized</param>
/// <param name="stream">destination stream</param>
let serialize prod_req (stream : MemoryStream ) =
  stream.write_int<RequiredAcks> prod_req.required_acks
  stream.write_int<Timeout> prod_req.timeout
  stream.write_int<ArraySize> prod_req.topic_data.Length
  prod_req.topic_data |> List.iter (serialize_topic_data stream)

let deserialize_partition_data (stream : MemoryStream) =
  { partition_id = stream.read_int32<PartitionId>()
    message_set_size = stream.read_int32<MessageSetSize> ()
    message_set = Message.deserialize stream }

let deserialize_partition_datas (stream : MemoryStream) =
  let num_partitions = stream.read_int32<ArraySize>()
  [for i in 1..num_partitions do yield deserialize_partition_data stream]

let deserialize_topic_data (stream : MemoryStream) =
  { topic_name = stream.read_str<StringSize> ()
    partition_data = deserialize_partition_datas stream }

let deserialize_topic_datas (stream : MemoryStream) =
  let num_topics = stream.read_int32<ArraySize> ()
  [for i in 1..num_topics do yield deserialize_topic_data stream ]

/// <summary>
///  Deserializes a ProduceRequest from the stream
/// </summary>
/// <param name="stream">the stream which contains a produce request</param>
let deserialize (stream : MemoryStream) : ProduceRequest =
  { required_acks = stream.read_int16<RequiredAcks>()
    timeout = stream.read_int32<Timeout>()
    topic_data = deserialize_topic_datas stream }