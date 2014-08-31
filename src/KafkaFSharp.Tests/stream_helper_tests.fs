module ElectroElephant.Tests.stream_helper_tests

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

    testCase "Serializing int32" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int<int32> 32
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let read_int = read_stream.read_int32<int32>()
      Assert.Equal("should be the same", 32, read_int)

    testCase "Serializing int16" <| fun _ ->
      use write_stream = new MemoryStream()
      write_stream.write_int<int16> 16s
      write_stream.Flush()
      use read_stream = new MemoryStream(write_stream.ToArray())
      let read_int = read_stream.read_int16<int16>()
      Assert.Equal("should be the same", 16s, read_int)
  ]

