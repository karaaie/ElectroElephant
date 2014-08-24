module KafkaFsharp.DecompressionTests

open Fuchu
open System.Text

open KafkaFSharp.Codecs

let EMPTY_STRING = Encoding.UTF8.GetBytes("")
let LONG_STRING = Encoding.UTF8.GetBytes("Hej kamil this will be 
  a very long text that hopefully should be a bit shorter when it is compressed
  by gzip.. woooohooooooooooo.")

[<Tests>]
let decompression =
  testList "" [
    testCase "Decompresseing non compressed data" <| fun _ ->
      Assert.Equal("should be the same size",
        LONG_STRING.Length = (decompress_data NO_COMPRESSION LONG_STRING).Length,
        true
      )

    testCase "Decompressing gzip compressed data" <| fun _ ->
      let compressed = compress_data GZIP_COMPRESSION LONG_STRING
      Assert.Equal("decompressed compressed data should be same size",
        LONG_STRING.Length = (decompress_data GZIP_COMPRESSION compressed).Length,
        true
      )
  ]

