module ElectroElephant.ConsumerMetadataRequest

open ElectroElephant.Common


// The offsets for a given consumer group are maintained by a 
// specific broker called the offset coordinator. i.e., a consumer 
// needs to issue its offset commit and fetch requests to this 
// specific broker. It can discover the current offset 
// coordinator by issuing a consumer metadata request.
type ConsumerMetadataRequest =
  {consumer_group : ConsumerGroup}

