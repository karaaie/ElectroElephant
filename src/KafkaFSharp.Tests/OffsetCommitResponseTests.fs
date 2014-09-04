module ElectroElephant.Tests.OffsetCommitResponseTests


open Fuchu

open ElectroElephant.OffsetCommitResponse
open ElectroElephant.Tests.StreamWrapperHelper

let partition_commit_responseA =
  { partition_id = 123
    error_code = 12s}

let topic_commit_responseA =
  { topic_name = "Tables"
    partition_commit_response = [partition_commit_responseA]}

let offset_commit =
  { topic_commit_response = [topic_commit_responseA]}

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and Deserialize should be the same" <| fun _ ->
      let result =
        stream_wrapper<OffsetCommitResponse> offset_commit serialize deserialize
      Assert.Equal("equal", offset_commit,result)
  ]
