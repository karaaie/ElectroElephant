module ElectroElephant.Tests.OffsetFetchResponse

open ElectroElephant.Common
open ElectroElephant.OffsetFetchResponse
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let partition_fetch_responseA = 
  { partition_id = 123
    offset = 123123L
    metadata = "Nonsens!!"
    error_code = 1s }

let offset_fetch_topicA = 
  { topic_name = "mooobiiiilen"
    partition_fetch_data = [ partition_fetch_responseA ] }

let offset_fetch = { topic_offset_data = [ offset_fetch_topicA ] }

[<Tests>]
let tests = 
  testList "" [ testCase "Deserialize and Serialize should be the same" <| fun _ -> 
                  let result = stream_wrapper<OffsetFetchResponse> offset_fetch serialize deserialize
                  Assert.Equal("equal", offset_fetch, result)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : OffsetFetchResponse) = 
                    stream_wrapper<OffsetFetchResponse> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]