module ElectroElephant.Tests.OffsetResponseTests

open Fuchu
open ElectroElephant.OffsetResponse
open ElectroElephant.Tests.StreamWrapperHelper



let partition_offset_dataA =
  { partition_id = 231231
    error_code = 2s
    offsets = [1L ; 123131231313121233L ; 5623423423L]}
    
let partition_offset_dataB =
  { partition_id = 1
    error_code = 2s
    offsets = [1L ; 3211223L ; 5623423423L]}

let partition_offset_dataC =
  { partition_id = 123123
    error_code = 2332s
    offsets = [1L ; 4443L ; 5623423423L]}

let topic_offset_dataA =
  { topic_name = "Grannies"
    partition_offset_data = 
      [partition_offset_dataA
       partition_offset_dataB
       partition_offset_dataC ]}

let offset_resp =
  { topic_offset_data = [topic_offset_dataA]}

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and Deserialize" <| fun _ ->
      let result =
        stream_wrapper<OffsetResponse> offset_resp serialize deserialize
      Assert.Equal("same", offset_resp, result)
  ]
