module KafkaFSharp.Model

type Message  =
  { magic        : byte
    compression  : byte
    checksum     : byte []
    payload      : byte []
    offset       : uint64
    payload_size : uint32 }

let message_equal msg1 msg2 : bool =
  msg1.magic = msg2.magic 
    && msg1.compression = msg2.compression
    && msg1.checksum = msg2.checksum
    && msg1.payload = msg2.payload
    && msg1.offset = msg2.offset
    && msg1.payload_size = msg2.payload_size
