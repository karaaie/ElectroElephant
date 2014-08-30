module ElectroElephant.MetadataRequest

open ElectroElephant.Common

type MetadataRequest =
  // The topics to produce metadata for. If empty the 
  // request will yield metadata for all topics.
  { topic_name : TopicName list }