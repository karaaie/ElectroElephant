module KafkaFSharp.Generator

open KafkaFSharp.Codecs
open KafkaFSharp.Message

let create_new_message (payload : byte []) (compression : Compression) =
  { magic = byte MAGIC_CONST
    compression = compression_id compression
    payload = compress_data compression payload
    checksum = [||]
    offset = 0UL
    totalLength = 0ul }
