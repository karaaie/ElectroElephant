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

type MessageOffset = int64

type Crc32 = int32
type MagicByte = int8
type MessageAttribute = int8
type MessageKey = byte []
type MessageValue = byte []

type TopicErrorCode = int16
type TopicName = string

type PartitionErrorCode = int16
type PartitionId = int32
type LeaderId = int32
type Replica = int32
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
    replicas : Replica list

    //The set subset of the replicas that are "caught up" to the leader
    isr : Isr list }

type TopicMetadata =
  { error_code  : TopicErrorCode
    name        : TopicErrorCode
    partitions  : PartitionMetadata list } 

type MetadataResponse =
  { brokers         : Broker list
    topic_metadatas : TopicMetadata list }

type RequestTypes =
  // Describes the currently available brokers, their host and port information,
  // and gives information about which broker hosts which partitions.
  | Metadata 
  //Send messages to a broker
  | Send
  //Fetch messages from a broker, one which fetches data, one which
  //gets cluster metadata, and one which gets offset information about a topic.
  | Fetch
  //Get information about the available offsets for a given topic partition.
  | Offsets
  //Commit a set of offsets for a consumer group
  | CommitOffset
  //Fetch a set of offsets for a consumer group
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