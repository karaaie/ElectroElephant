module FsharpKafka.Tests.Compression

open Fuchu
open System.Text

open KafkaFSharp.Compression

let EMPTY_STRING = Encoding.UTF8.GetBytes("")
let LONG_STRING = Encoding.UTF8.GetBytes("Hej kamil this will be 
  a very long text that hopefully should be a bit shorter when it is compressed
  by gzip.. woooohooooooooooo.")

[<Tests>]
let compression_tests =
  testList "Compression Tests" [

    testCase "Non compressed empty string" <| fun _ ->
      Assert.Equal(
        "should return empty array",
        compress_data Compression.NO_COMPRESSION EMPTY_STRING,
        [||])

    testCase "Compressed empty string" <| fun _ ->
      Assert.Equal(
        "should return empty array",
        compress_data Compression.GZIP_COMPRESSION EMPTY_STRING,
        [||])

    testCase "Non Compressed string" <| fun _ ->
      let compress = compress_data Compression.NO_COMPRESSION LONG_STRING
      Assert.Equal(
        "should be the same size",
        compress.Length, 
        LONG_STRING.Length)

    testCase "Compressed String" <| fun _ ->
      let compress = compress_data Compression.GZIP_COMPRESSION LONG_STRING
      Assert.Equal(
        "should be smaller",
        compress.Length < LONG_STRING.Length, 
        true)
  ]

