module ElectroElephant.Tests.ConsumerMetadataRequest

open ElectroElephant.Common
open ElectroElephant.ConsumerMetadataRequest
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let req = { consumer_group = "Lakritstrollen" }

[<Tests>]
let tests = 
  testList "" [ testCase "Serialize and Deserialize should be the same" <| fun _ -> 
                  let result = stream_wrapper<ConsumerMetadataRequest> req serialize deserialize
                  Assert.Equal("equal", req, result)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : ConsumerMetadataRequest) = 
                    stream_wrapper<ConsumerMetadataRequest> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]
