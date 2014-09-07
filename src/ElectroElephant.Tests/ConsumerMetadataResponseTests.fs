module ElectroElephant.Tests.ConsumerMetadataResponse

open ElectroElephant.Common
open ElectroElephant.ConsumerMetadataResponse
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let cons_md = 
  { error_code = 234s
    coordinator_id = 32
    coordinator_host = "www.nisse.se"
    coordinator_port = 453 }

[<Tests>]
let tests = 
  testList "" [ testCase "Serialized and Deserialzied should be the same" <| fun _ -> 
                  let result = stream_wrapper<ConsumerMetadataResponse> cons_md serialize deserialize
                  Assert.Equal("equal", cons_md, result)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : ConsumerMetadataResponse) = 
                    stream_wrapper<ConsumerMetadataResponse> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]