module ElectroElephant.MetadataRequest

open System.IO

open ElectroElephant.Common
open ElectroElephant.StreamHelpers

type MetadataRequest =
  // The topics to produce metadata for. If empty the 
  // request will yield metadata for all topics.
  { topic_names : TopicName list }

let serialize req (stream : MemoryStream) =
  req.topic_names |> stream.write_str_list
  ()

let deserialize (stream : MemoryStream) : MetadataRequest =
  let num_topics = stream.read_int32<ArraySize>()
  { topic_names = [for i in 1..num_topics do yield stream.read_str()] }