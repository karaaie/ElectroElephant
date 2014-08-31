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
  testList "Serialized Strings" [
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

  ]
  

