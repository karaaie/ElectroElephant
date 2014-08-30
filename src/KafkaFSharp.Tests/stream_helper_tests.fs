module ElectroElephant.Tests.stream_helper_tests

open System
open System.IO
open System.Text
open Microsoft.FSharp.Core.Operators

open Fuchu

open ElectroElephant.Common
open ElectroElephant.StreamHelpers



let test_string = "ElectroElephant"


[<Tests>]
let tests_str =
  testList "Serialized Strings" [
    testCase "Serialized and deserialized string should be the same" <| fun _ ->
      use stream = new MemoryStream()
      stream.write_str<StringSize> test_string
      stream.Flush()
      stream.Close()
      let str = stream.read_str<StringSize> ()
      Assert.Equal(
        "Serialized and deseralized string should be the same",
        test_string,
        str)

  ]
  

