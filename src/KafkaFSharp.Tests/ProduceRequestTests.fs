module ElectroElephant.Tests.ProduceRequestTests

open Fuchu

open ElectroElephant.Tests.StreamWrapperHelper
open ElectroElephant.Message
open ElectroElephant.ProduceRequest


let message_setA =
  { offset = 0L
    messages = []}

let partition_dataA =
  { partition_id = 1
    message_set_size = 13 
    message_set = message_setA}

let message_setB =
  { offset = 0L
    messages = []}

let partition_dataB =
  { partition_id = 1
    message_set_size = 13324 
    message_set = message_setB}

let message_setC =
  { offset = 0L
    messages = []}

let partition_dataC =
  { partition_id = 1
    message_set_size = 31213 
    message_set = message_setC}

let message_setD =
  { offset = 0L
    messages = []}

let partition_dataD =
  { partition_id = 221
    message_set_size = 13 
    message_set = message_setD}

let topic_dataA =
  { topic_name = "YelloCars"
    partition_data = [partition_dataA; partition_dataB]}

let topic_dataB =
  { topic_name = "RedBoats"
    partition_data = [partition_dataD; partition_dataC]}

let prod_req =
  { required_acks = 2s
    timeout = 2000
    topic_data = [topic_dataA; topic_dataB]}

[<Tests>]
let tests =
  testList "Serialization and Deserialization" [
    testCase "after and before should be the same" <| fun _ ->
      let result = 
        stream_wrapper<ProduceRequest> prod_req serialize deserialize
      Assert.Equal("should be the same", prod_req, result)
  ]
