﻿module KafkaFSharp.Compression

open System.IO
open System.IO.Compression

type Compression = 
  | NO_COMPRESSION
  | GZIP_COMPRESSION

//return the identity of the compression algorithm
let compression_to_id compression = 
  match compression with
  | NO_COMPRESSION -> byte 0
  | GZIP_COMPRESSION -> byte 1

let compression_from_id (b : byte) = 
  match b with
  | 0uy -> NO_COMPRESSION
  | 1uy -> GZIP_COMPRESSION
  | x -> failwith "unsupported compression type %d" x

let private compress_gzip (data : byte []) : byte [] = 
  use mem_stream = new MemoryStream()
  use gzip_stream = new GZipStream(mem_stream, CompressionMode.Compress)
  gzip_stream.Write(data, 0, data.Length)
  gzip_stream.Close()
  mem_stream.ToArray()

let private decompress_gzip (data : byte []) : byte [] = 
  use mem_stream = new MemoryStream(data)
  use gzip_stream = new GZipStream(mem_stream, CompressionMode.Decompress)
  use out_stream = new MemoryStream()
  gzip_stream.CopyTo(out_stream)
  out_stream.ToArray()

let private not_compressed (data : byte []) : byte [] = data
let private gzip_compress (data : byte []) : byte [] = compress_gzip data
let private gzip_decompress (data : byte []) : byte [] = decompress_gzip data

let compress_data compression data = 
  match compression with
  | NO_COMPRESSION -> not_compressed data
  | GZIP_COMPRESSION -> gzip_compress data

let decompress_data compression data = 
  match compression with
  | NO_COMPRESSION -> not_compressed data
  | GZIP_COMPRESSION -> gzip_decompress data