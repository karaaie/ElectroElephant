module ElectroElephant.Tests.OffsetResponseTests

open ElectroElephant.Common
open ElectroElephant.OffsetResponse
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

//
//type PartitionOffsetResponseData =
//  { partition_id : PartitionId 
//    error_code : ErrorCode
//    offsets : Offset list}
//
//type TopicOffsetResponseData =
//  { topic_name : TopicName
//    partition_offset_data : PartitionOffsetResponseData list}
//
//type OffsetResponse =
//  { topic_offset_data : TopicOffsetResponseData list}
//
let partition_offset_dataA = 
  { partition_id = 1231
    error_code = 123s
    offsets = [ 123L; 31231L; 6236236236L ] }

let partition_offset_dataB = 
  { partition_id = 1234141
    error_code = 12663s
    offsets = [ 127273L; 3272772721231L; 6236236236L ] }

let topic_offset_dataA = 
  { topic_name = "Unicorns"
    partition_offset_data = [ partition_offset_dataA; partition_offset_dataB ] }

let off_resp = { topic_offset_data = [ topic_offset_dataA ] }

[<Tests>]
let tests = 
  testList "" [ testCase "Serialized and Deserialized should be the same" <| fun _ -> 
                  let result = stream_wrapper<OffsetResponse> off_resp serialize deserialize
                  Assert.Equal("equal", off_resp, result)
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : OffsetResponse) = 
                    stream_wrapper<OffsetResponse> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]
