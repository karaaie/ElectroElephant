module ElectroElephant.Tests.MessageTests

open ElectroElephant.Common
open ElectroElephant.CompressionNew
open ElectroElephant.Message
open ElectroElephant.Tests.StreamWrapperHelper
open Fuchu

let message_a = 
  { crc = 32
    attributes = Compression.None
    magic_byte = 1y
    key = [| 3uy; 34uy; 65uy |]
    value = [| 3uy; 34uy; 65uy; 3uy; 34uy; 65uy; 3uy; 34uy; 65uy |] }

let message_b = 
  { crc = 22
    attributes = Compression.None
    magic_byte = 12y
    key = [| 3uy; 3uy; 65uy |]
    value = [| 3uy; 44uy; 65uy; 3uy; 134uy; 65uy; 3uy; 34uy; 65uy |] }

let message_c = 
  { crc = 32
    attributes = Compression.None
    magic_byte = 1y
    key = [| 3uy; 34uy; 5uy |]
    value = [| 3uy; 34uy; 65uy; 31uy; 34uy; 65uy; 3uy; 34uy; 65uy |] }

let message_d = 
  { crc = 32312
    attributes = Compression.None
    magic_byte = 12y
    key = [| 34uy; 34uy; 65uy |]
    value = [| 3uy; 34uy; 65uy; 3uy; 34uy; 65uy; 13uy; 34uy; 65uy |] }

let message_set = 
  { offset = 1L
    messages = [ message_a; message_b; message_c; message_d ] }

[<Tests>]
let tests = 
  testList "" [ testCase "" <| fun _ -> 
                  let result = stream_wrapper<MessageSet> message_set serialize deserialize
                  Assert.Equal("equal", message_set, result) ]
