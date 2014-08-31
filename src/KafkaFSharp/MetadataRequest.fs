module ElectroElephant.MetadataRequest

open System.IO

open ElectroElephant.Common
open ElectroElephant.StreamHelpers

type MetadataRequest =
  // The topics to produce metadata for. If empty the 
  // request will yield metadata for all topics.
  { topic_names : TopicName list }

let serialize req (stream : MemoryStream) =
  stream.write_str_list<ArraySize, StringSize> req.topic_names
  ()

let deserialize (stream : MemoryStream) : MetadataRequest =
  { topic_names = stream.read_str_list<ArraySize, StringSize> () }