module FsharpKafka.Tests.Compression

open FsCheck
open Fuchu
open KafkaFSharp.Compression
open System.Text

let EMPTY_STRING = Encoding.UTF8.GetBytes("")
let LONG_STRING = Encoding.UTF8.GetBytes("Hej kamil this will be 
  a very long text that hopefully should be a bit shorter when it is compressed
  by gzip.. woooohooooooooooo.")

[<Tests>]
let compression_tests = 
  testList "Compression Tests" [ testCase "Non compressed empty string" 
                                 <| fun _ -> 
                                   Assert.Equal
                                     ("should return empty array", compress_data Compression.NO_COMPRESSION EMPTY_STRING, 
                                      [||])
                                 
                                 testCase "Compressed empty string" 
                                 <| fun _ -> 
                                   Assert.Equal
                                     ("should return empty array", 
                                      compress_data Compression.GZIP_COMPRESSION EMPTY_STRING, [||])
                                 testCase "Non Compressed string" <| fun _ -> 
                                   let compress = compress_data Compression.NO_COMPRESSION LONG_STRING
                                   Assert.Equal("should be the same size", compress.Length, LONG_STRING.Length)
                                 testCase "Compressed String" <| fun _ -> 
                                   let compress = compress_data Compression.GZIP_COMPRESSION LONG_STRING
                                   Assert.Equal("should be smaller", compress.Length < LONG_STRING.Length, true) ]

let non_null_str (s : string) = 
  Encoding.UTF8.GetBytes(s)
  |> compress_data Compression.NO_COMPRESSION
  |> decompress_data Compression.NO_COMPRESSION
  |> fun s -> Encoding.UTF8.GetString(s)

let comp_decomp_no_compression (str : string) = 
  if str <> null then str = non_null_str str
  else "" = non_null_str ""

[<Tests>]
let compress_decompress = 
  testList "" 
    [ testCase "compressing and decompressing the string should return the same string" 
      <| fun _ -> Check.QuickThrowOnFailure comp_decomp_no_compression ]