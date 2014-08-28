module KafkaFSharp.Message


//Fixed Width Primitives
//
//int8, int16, int32, int64 - Signed integers with the given precision (in bits) stored in big endian order.
//
//Variable Length Primitives
//
//bytes, string - These types consist of a signed integer giving a length N followed by N bytes of content. A length of -1 indicates null. string uses an int16 for its size, and bytes uses an int32.
//
//Arrays
//
//This is a notation for handling repeated structures. These will always be 
//encoded as an int32 size containing the length N followed by N repetitions of 
//the structure which can itself be made up of other primitive types. In the BNF 
//grammars below we will show an array of a structure foo as [foo].


type MessageSize = int32
type ApiKey = int16
type ApiVersion = int16
type CorrelationId = int32
type ClientId = string

type Offset = int64

type Crc32 = int32
type MagicByte = int8
type MessageAttribute = int8
type MessageKey = byte []
type MessageValue = byte []

type ErrorCode = int16
type TopicName = string

type PartitionId = int32
type LeaderId = int32
type ReplicaId = int32
type Isr = int32


type NodeId = int32
type Hostname = string
type Port = int32

// Kafka supports compressing messages for additional efficiency, 
// however this is more complex than just compressing a raw message. 
// Because individual messages may not have sufficient redundancy 
// to enable good compression ratios, compressed messages must 
// be sent in special batches (although you may use a batch of one 
// if you truly wish to compress a message on its own). The messages
// to be sent are wrapped (uncompressed) in a MessageSet structure, 
// which is then compressed and stored in the Value field of a 
// single "Message" with the appropriate compression codec set. 
// The receiving system parses the actual MessageSet from the decompressed value.
//   Message { 
//   Compression = Snappy, 
//   Value = MessageSet of N Messages, each message is uncompressed in this set.} 
type Compression =
  | None of MessageAttribute
  | GZIP of MessageAttribute
  | Snappy of MessageAttribute

type Message =
  // The CRC is the CRC32 of the remainder of the message
  // bytes. This is used to check the integrity of 
  // the message on the broker and consumer.
  { crc         : Crc32

   // This is a version id used to allow backwards compatible evolution of the message binary format.
    magic_byte  : MagicByte

    // This byte holds metadata attributes about the
    //  message. The lowest 2 bits contain the compression codec 
    // used for the message. The other bits should be set to 0.
    attributes  : Compression

    // The key is an optional message key that was 
    // used for partition assignment. The key can be null.
    key         : MessageKey

    // The value is the actual message contents as an opaque 
    // byte array. Kafka supports recursive messages in 
    // which case this may itself contain a 
    // message set. The message can be null.
    value       : MessageValue }

type MessageSet =
  //This is the offset used in kafka as the log sequence number. 
  // When the producer is sending messages it doesn't actually 
  // know the offset and can fill in any value here it likes.
  { offset    : MessageOffset
    messages  : Message list }



type MetadataRequest =
  // The topics to produce metadata for. If empty the 
  // request will yield metadata for all topics.
  { topic_name : string list }

type Broker =
  { node_id : NodeId
    host    : Hostname
    port    : Port }

type PartitionMetadata = 
  { error_code : PartitionErrorCode
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
  { error_code  : TopicErrorCode
    name        : TopicErrorCode
    partitions  : PartitionMetadata list } 

type MetadataResponse =
  { brokers         : Broker list
    topic_metadatas : TopicMetadata list }


//    ProduceRequest => RequiredAcks Timeout [TopicName [Partition MessageSetSize MessageSet]]
//  RequiredAcks => int16
//  Timeout => int32
//  Partition => int32
//  MessageSetSize => int32


type RequiredAcks = int16
type Timeout = int32
type MessageSetSize = int32

type PartitionData =
    // The partition that data is being published to.
  { partion_id : PartitionId
    // The size, in bytes, of the message set that follows.
    message_set_size : MessageSetSize
    // A set of messages in the standard format described above.
    message_set : MessageSet }

type TopicData =
    // The topic that data is being published to.
  { topic_name : TopicName
    partition_data : PartitionData list }
    
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


//FetchRequest => ReplicaId MaxWaitTime MinBytes [TopicName [Partition FetchOffset MaxBytes]]
//  ReplicaId => int32
//  MaxWaitTime => int32
//  MinBytes => int32
//  TopicName => string
//  Partition => int32
//  FetchOffset => int64
//  MaxBytes => int32

type MaxWaitTime = int32
type MinBytes = int32
type MaxBytes = int32


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

type RequestTypes =
  // Describes the currently available brokers, their host and port information,
  // and gives information about which broker hosts which partitions.
  | Metadata of MetadataRequest
  // Send messages to a broker
  | Send of ProduceRequest
  // Fetch messages from a broker, one which fetches data, one which
  // gets cluster metadata, and one which gets offset information about a topic.
  | Fetch of FetchRequest
  // Get information about the available offsets for a given topic partition.
  | Offsets
  // Commit a set of offsets for a consumer group
  | CommitOffset
  // Fetch a set of offsets for a consumer group
  | FetchOffset

type Request =
  // This is a numeric id for the API being invoked (i.e. is it a 
  // metadata request, a produce request, a fetch request, etc).
  { api_key : ApiKey

  // This is a numeric version number for this api. We version each 
  // API and this version number allows the server to properly interpret
  // the request as the protocol evolves. Responses will always be in 
  // the format corresponding to the request version. 
  // Currently the supported version for all APIs is 0.
    api_version : ApiVersion

  // This is a user-supplied integer. It will be passed back in the response
  // by the server, unmodified. It is useful for matching request and 
  // response between the client and server.
    correlation_id : CorrelationId

  // This is a user supplied identifier for the client application. The user 
  // can use any identifier they like and it will be used when logging
  // errors, monitoring aggregates, etc. For example, one might want to 
  // monitor not just the requests per second overall, but the number 
  // coming from each client application (each of which could reside on 
  // multiple servers). This id acts as a logical grouping across all requests from a particular client.
    client_id : ClientId

  // which API this request targets.
    request_type : RequestTypes }

type ResponseTypes =
  | Metadata
  | Produce
  | Fetch
  | Offset
  | CommitOffset
  | FetchOffset

type Response =
  // The server passes back whatever integer the client supplied as 
  // the correlation in the request.
  { correlation_id : CorrelationId

    response_type : ResponseTypes }

type DirectionType =
  // To Kafka
  | Request of Request

  // From Kafka
  | Response of Response

type RequestOrResponse =
  //The MessageSize field gives the size of the subsequent request or response 
  //message in bytes. The client can read requests by first reading this 4 byte size 
  //as an integer N, and then reading and parsing the subsequent N bytes of the request.
  { size : MessageSize

  // declares if this is a Response or Request
    message_type : DirectionType }



    //https://cwiki.apache.org/confluence/display/KAFKA/A+Guide+To+The+Kafka+Protocol

    // NEXT: Produce API