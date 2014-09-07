module ElectroElephant.Tests.MetadataRequestTests

open ElectroElephant.MetadataRequest
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let meta_req = { topic_names = [ "Sugar"; "Icecream"; "Other fun stuff" ] }

[<Tests>]
let metadata_req_tests = 
  testList "" [ testCase "Serialization and deserialization should not alter content" <| fun _ -> 
                  let result = stream_wrapper<MetadataRequest> meta_req serialize deserialize
                  Assert.Equal("same topics should be present", meta_req.topic_names, result.topic_names)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : MetadataRequest) = 
                    stream_wrapper<MetadataRequest> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]
