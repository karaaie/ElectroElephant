module KafkaFSharp.Generator

open KafkaFSharp.Crc32
open KafkaFSharp.Model
open KafkaFSharp.Compression
open KafkaFSharp.Encoding

open System

let create_new_message (input_payload : byte []) (compression : Compression) =
  let compressed_payload = compress_data compression input_payload
  { magic = byte MAGIC_CONST
    compression = compression_to_id compression
    payload = compressed_payload
    checksum = crc32 input_payload |> BitConverter.GetBytes
    offset = 0UL
    payload_size = compressed_payload.Length |> uint32 }
