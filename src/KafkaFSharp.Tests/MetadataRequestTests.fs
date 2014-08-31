module ElectroElephant.Tests.MetadataRequestTests

open Fuchu

open ElectroElephant.MetadataRequest

open ElectroElephant.Tests.StreamWrapperHelper

let meta_req = 
  { topic_names = ["Sugar"; "Icecream"; "Other fun stuff"]}

[<Tests>]
let metadata_req_tests =
  testList "" [
    testCase "Serialization and deserialization should not alter content" <| fun _ ->
      let result = 
        stream_wrapper<MetadataRequest> meta_req serialize deserialize 
      Assert.Equal(
        "same topics should be present",
        meta_req.topic_names, result.topic_names)
  ]

