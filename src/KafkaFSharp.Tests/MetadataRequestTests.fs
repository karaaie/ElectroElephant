module ElectroElephant.Tests.MetadataRequestTests

open System.IO
open Fuchu

open ElectroElephant.Common
open ElectroElephant.MetadataRequest

let meta_req = 
  { topic_names = ["Sugar"; "Icecream"; "Other fun stuff"]}

[<Tests>]
let metadata_req_tests =
  testList "" [
    testCase "Serialization and deserialization should not alter content" <| fun _ ->
      use write_stream = new MemoryStream()
      serialize meta_req write_stream 
      use read_stream = new MemoryStream(write_stream.ToArray())
      let result = deserialize read_stream
      Assert.Equal(
        "same topics should be present",
        meta_req.topic_names, result.topic_names)
  ]

