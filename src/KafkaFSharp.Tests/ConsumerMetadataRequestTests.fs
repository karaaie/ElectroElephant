module ElectroElephant.Tests.ConsumerMetadataRequest

open Fuchu

open ElectroElephant.Tests.StreamWrapperHelper
open ElectroElephant.Common
open ElectroElephant.ConsumerMetadataRequest

let req = { consumer_group = "Lakritstrollen" }

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and Deserialize should be the same" <| fun _ ->
      let result =
        stream_wrapper<ConsumerMetadataRequest> req serialize deserialize
      Assert.Equal("equal", req, result)
  ]

