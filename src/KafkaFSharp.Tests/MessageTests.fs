module ElectroElephant.Tests.MessageTests

open ElectroElephant.Common
open ElectroElephant.CompressionNew
open ElectroElephant.Message
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
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
  testList "" [ testCase "Serialize and deserialize should be the same" <| fun _ -> 
                  let result = stream_wrapper<MessageSet> message_set serialize deserialize
                  Assert.Equal("equal", message_set, result) //]
                testCase "FsCheck test" <| fun _ -> 
                  let msg_set_fs (msg_set : MessageSet) = 
                    stream_wrapper<MessageSet> msg_set serialize deserialize = msg_set
                  Check.QuickThrowOnFailure msg_set_fs ]
// For future reference
//let create_message crc magic attribute key value = 
//  { crc = crc
//    magic_byte = magic
//    attributes = attribute
//    key = key
//    value = value }
//
//let generate_crc() = Arb.generate<Crc32>
//let generate_magic_byte() = Arb.generate<MagicByte>
//let generate_attribute() = Arb.generate<Compression>
//let generate_key() = Arb.generate<byte []>
//let generate_value() = Arb.generate<byte []>
//let generate_message() = 
//  create_message <!> generate_crc() <*> generate_magic_byte() <*> generate_attribute() <*> generate_key() 
//  <*> generate_value()
//let generate_offset() = Arb.generate<Offset>
//let generate_messages() = Gen.listOf (generate_message())
//
//let create_message_set offset messages = 
//  { offset = offset
//    messages = messages }
//
//let generate_message_set = create_message_set <!> generate_offset() //<*> generate_messages()
//type MessageSetGen = 
//  static member MessageSet() = 
//    { new Arbitrary<MessageSet>() with
//        member x.Generator = generate_message_set
//        member x.Shrinker t = Seq.empty }
//
//Arb.register<MessageSetGen>() |> ignore