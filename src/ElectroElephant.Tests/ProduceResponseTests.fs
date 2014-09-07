module ElectroElephant.Tests.ProduceResponseTests

open ElectroElephant.Message
open ElectroElephant.ProduceResponse
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let prod_partition_respA = 
  { partition_id = 1
    error_code = 1s
    offset = 123L }

let prod_topic_respA = 
  { topic_name = "MegaTrucks"
    partition_responses = [ prod_partition_respA ] }

let prod_topic_respB = 
  { topic_name = "MegaTrucks"
    partition_responses = [] }

let prod_resp = { topic_responses = [ prod_topic_respB ] }

[<Tests>]
let tests = 
  testList "Serialize and Deserialize" [ testCase "should be the same before and after" <| fun _ -> 
                                           let result = stream_wrapper<ProduceResponse> prod_resp serialize deserialize
                                           Assert.Equal("same", prod_resp, result)
                                         testCase "FsCheck test" <| fun _ -> 
                                           let msg_set_fs (msg_set : ProduceResponse) = 
                                             stream_wrapper<ProduceResponse> msg_set serialize deserialize = msg_set
                                           Check.QuickThrowOnFailure msg_set_fs ]
