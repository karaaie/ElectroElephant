module ElectroElephant.Tests.FetchRequestTests

open Fuchu

open ElectroElephant.Common
open ElectroElephant.FetchRequest
open ElectroElephant.Tests.StreamWrapperHelper

let fetch_partition_data =
  { partition_id = 123
    fetch_offset = 12312313123123L
    max_bytes = 4322}

let fetch_topic_data =
  { topic_name = "music"
    partition_data = [fetch_partition_data]}

let fetch_req =
  { replica_id = -1
    max_wait_time = 2000
    min_bytes = 2048
    topic_data = [fetch_topic_data]}

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and deserialize should be equal" <| fun _ ->
      let result =
        stream_wrapper<FetchRequest> fetch_req serialize deserialize
      Assert.Equal("equal", fetch_req, result)
  ]
