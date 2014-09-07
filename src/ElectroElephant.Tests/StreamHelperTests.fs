module ElectroElephant.Tests.StreamHelperTests

open System
open System.IO
open System.Text
open Microsoft.FSharp.Core.Operators

open Fuchu

open ElectroElephant.Common
open ElectroElephant.StreamHelpers



let test_string = "ElectroElephant"
let test_string_array = ["ElectroElephant"; "Kafka"; "FSharp"; "Visual Studio"]


[<Tests>]
let tests_str =
  testList "Strings serialization" [
    testCase "Serialized and deserialized string should be the same" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_str<StringSize> test_string
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let str = read_stream.read_str<StringSize> ()
      Assert.Equal("should be the same", test_string, str)

    testCase "Serialized and deserialized string array " <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_str_list<ArraySize, StringSize> test_string_array
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let str_arr = read_stream.read_str_list<ArraySize, StringSize> ()
      Assert.Equal(
        "should not be different", 
        test_string_array <> str_arr, 
        false)

    testCase "Null string" <| fun _ -> 
      use write_stream = new MemoryStream()
      write_stream.write_str<StringSize> null
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let null_str = read_stream.read_str<StringSize> ()
      Assert.Equal("should not throw error", null, null_str)
  ]


let int16_list = [1s; 2s; 3s; 4s; 5s]
let int32_list = [6; 7; 8; 9; 10]
let int64_list = [11L; 12L; 13L; 14L; 15L]

[<Tests>]
let tests_ints =
  testList "" [
    testCase "Serializing int64" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int<int64> 64L
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let read_int = read_stream.read_int64<int64>()
      Assert.Equal("should be the same", 64L, read_int)

    testCase "Serialize int64 list" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int_list<ArraySize,int64> int64_list
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let ints = read_stream.read_int64_list<ArraySize,int64> ()
      Assert.Equal("should be equal", int64_list, ints)

    testCase "Serializing int32" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int<int32> 32
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let read_int = read_stream.read_int32<int32>()
      Assert.Equal("should be the same", 32, read_int)

    testCase "Serialize int32 list" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int_list<ArraySize,int32> int32_list
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let ints = read_stream.read_int32_list<ArraySize,int32> ()
      Assert.Equal("should be equal", int32_list, ints)

    testCase "Serializing int16" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int<int16> 16s
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let read_int = read_stream.read_int16<int16>()
      Assert.Equal("should be the same", 16s, read_int)

    testCase "Serialize int16 list" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int_list<ArraySize,int16> int16_list
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let ints = read_stream.read_int16_list<ArraySize,int16> ()
      Assert.Equal("should be equal", int16_list, ints)
  ]

