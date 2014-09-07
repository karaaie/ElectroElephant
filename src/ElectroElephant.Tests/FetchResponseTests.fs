module ElectroElephant.Tests.FetchResponseTests

open ElectroElephant.FetchResponse
open ElectroElephant.Message
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let empty_message_set = 
  { offset = 0L
    messages = [] }

let fetch_partitionA = 
  { partition_id = 1
    error_code = 1s
    highwater_mark_offset = 123123L
    message_set_size = 321
    message_set = empty_message_set }

let fetch_partitionB = 
  { partition_id = 131231
    error_code = 221s
    highwater_mark_offset = 1231312323L
    message_set_size = 34421
    message_set = empty_message_set }

let fetch_partitionC = 
  { partition_id = 13
    error_code = 1s
    highwater_mark_offset = 123123L
    message_set_size = 3221
    message_set = empty_message_set }

let fetch_partitionD = 
  { partition_id = 13
    error_code = 1s
    highwater_mark_offset = 123123L
    message_set_size = 3521
    message_set = empty_message_set }

let fetch_topicA = 
  { topic_name = "FishyFishes"
    partition_messages = [ fetch_partitionA; fetch_partitionB; fetch_partitionC ] }

let fetch_topicB = 
  { topic_name = "EvilSantaClause"
    partition_messages = [ fetch_partitionD ] }

let fetch_resp = { topic_messages = [ fetch_topicA; fetch_topicB ] }

[<Tests>]
let tests = 
  testList "Serialize and Deserialize" [ testCase "should be the same before and after" <| fun _ -> 
                                           let result = 
                                             stream_wrapper<FetchResponse> fetch_resp 
                                               ElectroElephant.FetchResponse.serialize 
                                               ElectroElephant.FetchResponse.deserialize
                                           Assert.Equal("should be the same", fetch_resp, result)
                                         testCase "FsCheck test" <| fun _ -> 
                                           let msg_set_fs (msg_set : FetchResponse) = 
                                             stream_wrapper<FetchResponse> msg_set 
                                               ElectroElephant.FetchResponse.serialize 
                                               ElectroElephant.FetchResponse.deserialize = msg_set
                                           Check.QuickThrowOnFailure msg_set_fs ]
