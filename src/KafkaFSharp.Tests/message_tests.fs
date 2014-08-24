module KafkaFSharp.message_tests

open KafkaFSharp.Model
open KafkaFSharp.Generator
open KafkaFSharp.Compression

open Fuchu
open System
open System.Text

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

let msg_bytes = Encoding.UTF8.GetBytes("looooong text with mucho importante stuff in it,
  gargerglls garaasdflkasdf; asdflaksdfasdfmn mmcmcmcjsjhadjfa sdfasld;kfjakls;df
  asdflaksdf;kasd;lfksadf , , as;ldfkasdfk;asdf,,,, vmmmvnkfvlf
")

let normal_message = create_new_message msg_bytes NO_COMPRESSION
let gzipped_message = create_new_message msg_bytes GZIP_COMPRESSION

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

    testCase "normal created message" <| fun _ ->
      Assert.Equal("should have same payload size as original data",
        normal_message.payload_size,
        uint32 msg_bytes.Length)

    testCase "normal created message" <| fun _ ->
      Assert.Equal("should have correct checksum",
        normal_message.checksum,
        [|194uy; 231uy; 103uy; 130uy|])

    testCase "normal created message" <| fun _ -> 
      Assert.Equal("should have no compression flag set",
        normal_message.compression,
        compression_to_id Compression.NO_COMPRESSION)

    testCase "gzipped and created message" <| fun _ ->
      Assert.Equal("should have smaller payload size as original data",
        gzipped_message.payload_size < uint32 msg_bytes.Length,
        true)

    testCase "gzipped and created message" <| fun _ ->
      Assert.Equal("should have correct checksum",
        gzipped_message.checksum,
        [|170uy; 137uy; 203uy; 6uy|])

    testCase "gzipped and created message" <| fun _ -> 
      Assert.Equal("should have no compression flag set",
        gzipped_message.compression,
        compression_to_id Compression.GZIP_COMPRESSION)
  ]




























