module ElectroElephant.Tests.OffsetRequestTests

open Fuchu

open ElectroElephant.Common
open ElectroElephant.OffsetRequest
open ElectroElephant.Tests.StreamWrapperHelper

//[<StructuralEquality;StructuralComparison>]
//type PartitionFetchRequestData =
//  { partition_id : PartitionId
//    time : Time 
//    max_number_of_offsets : MaxNumberOfOffsets }
//
//[<StructuralEquality;StructuralComparison>]
//type TopicOffsetRequestData =
//  { topic_name : TopicName
//    partition_fetch_data : PartitionFetchRequestData list }
//
//[<StructuralEquality;StructuralComparison>]
//type OffsetRequest = 
//  { replica_id : ReplicaId
//    topic_offset_data : TopicOffsetRequestData list}

let partition_offset =
  { partition_id = 123
    time = 131231313213L
    max_number_of_offsets = 123123}

let topic_offset =
  { topic_name = "lingonsylt"
    partition_fetch_data = [partition_offset]}

let offset_req =
  { replica_id = -1
    topic_offset_data = [topic_offset]}

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and Deserialize should be the same" <| fun _ ->
      let result =
        stream_wrapper<OffsetRequest> offset_req serialize deserialize
      Assert.Equal("equal", offset_req, result)
  ]
