module ElectroElephant.Common

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


type StringSize = int16
type ByteArraySize = int32
type ArraySize = int32

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

type MaxWaitTime = int32
type MinBytes = int32
type MaxBytes = int32

type RequiredAcks = int16
type Timeout = int32
type MessageSetSize = int32

type Time = int64
type MaxNumberOfOffsets = int32

type ConsumerGroup = string
type CoordinatorId = int32

type Metadata = string

// The following are the numeric codes that 
// the ApiKey in the request can take for each of the above request types.
type ApiKeys =
  | ProduceRequest
  | FetchRequest
  | OffsetRequest
  | MetaDataRequest
  | OffsetCommitRequest
  | OffsetFetchRequest
  | ConsumerMetadataRequest
  | InternalApi

let number_to_api_key number =
  match number with 
  | 0 -> ProduceRequest
  | 1 -> FetchRequest
  | 2 -> OffsetRequest
  |  3 -> MetaDataRequest
  | 4 | 5 | 6 | 7 -> InternalApi
  | 8 -> OffsetCommitRequest
  | 9 -> OffsetFetchRequest
  | 10 -> ConsumerMetadataRequest
  | wrong -> failwithf "unknown api number %d" wrong

let api_key_to_number key =
  match key with
  | ProduceRequest -> 0
  | FetchRequest -> 1
  | OffsetRequest -> 2
  | MetaDataRequest -> 3
  | OffsetCommitRequest -> 8
  | OffsetFetchRequest -> 9
  | ConsumerMetadataRequest -> 10
  | InternalApi -> 4


type ErrorCodes =
  // 0
  //No error--it worked!
  | NoError

  // -1
  // An unexpected server error
  | Unknown

  // 1
  // The requested offset is outside the range of 
  // offsets maintained by the server for the given topic/partition.
  | OffsetOutOfRange

  // 2
  // This indicates that a message contents does not match its CRC
  | InvalidMessage

  // 3
  // This request is for a topic or partition that does not exist on this broker.
  | UnknownTopicOrPartition

  // 4
  // The message has a negative size
  | InvalidMessageSize

  // 5
  // This error is thrown if we are in the middle of a leadership 
  // election and there is currently no leader for this partition 
  // and hence it is unavailable for writes.
  | LeaderNotAvailable

  // 6
  // This error is thrown if the client attempts to send messages 
  // to a replica that is not the leader for some partition. It 
  // indicates that the clients metadata is out of date.
  | NotLeaderForPartition

  // 7
  // This error is thrown if the request exceeds the 
  // user-specified time limit in the request.
  | RequestTimedOut

  // 8
  // This is not a client facing error and is used mostly by tools when a broker is not alive.
  | BrokerNotAvailable

  // 9
  // If replica is expected on a broker, but is not.
  | ReplicaNotAvailable

  // 10
  // The server has a configurable maximum message size to avoid 
  // unbounded memory allocation. This error is thrown if 
  // the client attempt to produce a message larger than this maximum.
  | MessageSizeTooLarge

  // 11
  // Internal error code for broker-to-broker communication.
  | StaleControllerEpochCode

  // 12
  // If you specify a string larger than configured maximum for offset metadata
  | OffsetMetadataTooLargeCode

  // 14
  // The broker returns this error code for an offset fetch 
  // request if it is still loading offsets (after a leader 
  // change for that offsets topic partition).
  | OffsetsLoadInProgressCode

  // 15 
  // The broker returns this error code for consumer metadata 
  // requests or offset commit requests if the 
  // offsets topic has not yet been created.
  | ConsumerCoordinatorNotAvailableCode

  // 16
  // The broker returns this error code if it receives an offset 
  // fetch or commit request for a consumer group 
  // that it is not a coordinator for.
  | NotCoordinatorForConsumerCode

let number_to_error number =
  match number with
  | 0 -> NoError
  | -1 -> Unknown
  | 1 -> OffsetOutOfRange
  | 2 -> InvalidMessage
  | 3 ->  UnknownTopicOrPartition
  | 4 -> InvalidMessageSize
  | 5 -> LeaderNotAvailable
  | 6 -> NotLeaderForPartition
  | 7 -> RequestTimedOut
  | 8 -> BrokerNotAvailable
  | 9 -> ReplicaNotAvailable
  | 10 -> MessageSizeTooLarge
  | 11 -> StaleControllerEpochCode
  | 12 -> OffsetMetadataTooLargeCode
  | 14 -> OffsetsLoadInProgressCode
  | 15 -> ConsumerCoordinatorNotAvailableCode
  | 16 -> NotCoordinatorForConsumerCode
  | wrong -> failwithf "unknown error code %d " wrong

let error_to_number error =
  match error with
  | NoError -> 0
  | Unknown -> -1
  | OffsetOutOfRange -> 1 
  | InvalidMessage -> 2
  | UnknownTopicOrPartition -> 3
  | InvalidMessageSize -> 4
  | LeaderNotAvailable -> 5
  | NotLeaderForPartition -> 6
  | RequestTimedOut -> 7
  | BrokerNotAvailable -> 8
  | ReplicaNotAvailable -> 9
  | MessageSizeTooLarge -> 10
  | StaleControllerEpochCode -> 11
  | OffsetMetadataTooLargeCode -> 12
  | OffsetsLoadInProgressCode -> 14
  | ConsumerCoordinatorNotAvailableCode -> 15
  | NotCoordinatorForConsumerCode -> 16
