module ElectroElephant.Tests.OffsetCommitRequestTests

open ElectroElephant.Common
open ElectroElephant.OffsetCommitRequest
open ElectroElephant.Tests.StreamWrapperHelper
open Fuchu

let partition_offsetA = 
    { partition_id = 12312
      partition_offset = 4545454545L
      timestamp = -1L
      metadata = "Im done!" }

let partition_offsetB = 
    { partition_id = 2
      partition_offset = 45L
      timestamp = -1L
      metadata = "Im not done!" }

let topic_offsetA = 
    { topic_name = "Maskin"
      partition_offset_commit = [ partition_offsetA ] }

let topic_offsetB = 
    { topic_name = "Maskin"
      partition_offset_commit = [ partition_offsetB ] }

let offset_commit = 
    { consumer_group = "Nollan"
      topic_offset_commit = [ topic_offsetA; topic_offsetB ] }

[<Tests>]
let tests = 
    testList "" [ testCase "Serialized and Deserialized should be equal" <| fun _ -> 
                      let result = stream_wrapper<OffsetCommitRequest> offset_commit serialize deserialize
                      Assert.Equal("equal", offset_commit, result) ]
