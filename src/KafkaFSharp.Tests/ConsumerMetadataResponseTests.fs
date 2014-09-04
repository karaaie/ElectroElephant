module ElectroElephant.Tests.ConsumerMetadataResponse

open Fuchu

open ElectroElephant.Common
open ElectroElephant.ConsumerMetadataResponse
open ElectroElephant.Tests.StreamWrapperHelper

let cons_md = 
  { error_code = 234s
    coordinator_id = 32
    coordinator_host = "www.nisse.se"
    coordinator_port = 453}

[<Tests>]
let tests = 
  testList "" [
    testCase "Serialized and Deserialzied should be the same" <| fun _ ->
      let result =
        stream_wrapper<ConsumerMetadataResponse> cons_md serialize deserialize
      Assert.Equal("equal", cons_md, result)
  ]