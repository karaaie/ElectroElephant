module KafkaFSharp.Codecs

open System.IO.Compression
open System.IO

type Compression = 
  NO_COMPRESSION
  | GZIP_COMPRESSION

// Byte[] => MemStream => GZip it => MemStream => Byte []
let gzip_compress (data : byte []) : byte [] = 
  use mem_stream = new MemoryStream()
  use gzip_stream = new GZipStream(mem_stream, CompressionMode.Compress)
  gzip_stream.Write(data, 0, data.Length)
  gzip_stream.Close()
  mem_stream.ToArray()

let private not_compressed (data : byte []) : byte [] =
  data

let private gzip_compressed (data : byte []) : byte [] =
  gzip_compress data

//return the identity of the compression algorithm
let compression_id compression =
  match compression with
  | NO_COMPRESSION -> byte 0
  | GZIP_COMPRESSION -> byte 1

//compress data 
let compress_data compression data =
  match compression with
  | NO_COMPRESSION -> not_compressed data
  | GZIP_COMPRESSION -> gzip_compressed data

//let 

//type NoCompressionPayloadCodec struct {
//}
//func (codec *NoCompressionPayloadCodec) Id() byte {
//  return NO_COMPRESSION_ID
//}
//func (codec *NoCompressionPayloadCodec) Encode(data []byte) []byte {
//  return data
//}
//func (codec *NoCompressionPayloadCodec) Decode(data []byte) []byte {
//  return data
//}
//// Gzip Codec
//type GzipPayloadCodec struct {
//}
//func (codec *GzipPayloadCodec) Id() byte {
//return GZIP_COMPRESSION_ID
//}
//func (codec *GzipPayloadCodec) Encode(data []byte) []byte {
//buf := bytes.NewBuffer([]byte{})
//zipper, _ := gzip.NewWriterLevel(buf, gzip.BestSpeed)
//zipper.Write(data)
//zipper.Close()
//return buf.Bytes()
//}
//func (codec *GzipPayloadCodec) Decode(data []byte) []byte {
//buf := bytes.NewBuffer([]byte{})
//zipper, _ := gzip.NewReader(bytes.NewBuffer(data))
//unzipped := make([]byte, 100)
//for {
//n, err := zipper.Read(unzipped)
//if n > 0 && err == nil {
//buf.Write(unzipped[0:n])
//} else {
//break
//}
//}
//zipper.Close()
//return buf.Bytes()
//}
//
