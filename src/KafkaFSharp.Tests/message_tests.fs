module KafkaFSharp.message_tests

open KafkaFSharp.Model
open KafkaFSharp.Generator
open KafkaFSharp.Compression

open Fuchu
open System

let empty_message_a =
  { payload = [||]
    magic = 0uy
    compression = 0uy
    checksum = [||]
    offset = uint64 0
    payload_size = uint32 0 }

let empty_message_b =
  { payload = [||]
    magic = 0uy
    compression = 0uy
    checksum = [||]
    offset = uint64 0
    payload_size = uint32 0 }

let empty_created_message = create_new_message [||] NO_COMPRESSION

[<Tests>]
let message_tests =
  testList "" [
    testCase "empty messages" <| fun _ ->
      Assert.Equal("should be equal",
        empty_message_a,
        empty_message_b)

    testCase "empty created message" <| fun _ ->
      Assert.Equal("should have zero payload size", 
        empty_created_message.payload_size,
        uint32 0)

    testCase "empty created message" <| fun _ ->
      Assert.Equal("should have no payload",
        empty_created_message.payload,
        [||])

    testCase "empty created message" <| fun _ ->
      Assert.Equal("should have an empty checksum",
         empty_created_message.checksum,
         [|0uy;  0uy;  0uy; 0uy|])
  ]

