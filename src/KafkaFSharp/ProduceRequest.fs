module ElectroElephant.ProduceRequest

open ElectroElephant.Common
open ElectroElephant.Message

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
