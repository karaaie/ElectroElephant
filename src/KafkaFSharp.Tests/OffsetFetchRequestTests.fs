module ElectroElephant.Tests.OffsetFetchRequest

open ElectroElephant.Common
open ElectroElephant.OffsetFetchRequest
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let topic_offsetB = 
  { topic_name = "InteKorvar"
    partitions = [ 1; 3; 65 ] }

let topic_offsetA = 
  { topic_name = "Korvar"
    partitions = [ 1; 4; 634; 2 ] }

let offset_req = 
  { consumer_group = "NissesKoravMoj"
    topic_offset_data = [ topic_offsetA; topic_offsetB ] }

[<Tests>]
let tests = 
  testList "" [ testCase "Serialized and Deserialized should be equal" <| fun _ -> 
                  let result = stream_wrapper<OffsetFetchRequest> offset_req serialize deserialize
                  Assert.Equal("equal", offset_req, result)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : OffsetFetchRequest) = 
                    stream_wrapper<OffsetFetchRequest> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]