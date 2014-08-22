module KafkaFSharp.Message


// Compression Support uses '1' - https://cwiki.apache.org/confluence/display/KAFKA/Compression
let MAGIC_CONST = 1

// magic + compression + chksum
let NO_LEN_HEADER_SIZE = MAGIC_CONST + 1 + 4

type Message  = 
  { magic       : byte
    compression : byte
    checksum    : byte[]
    payload     : byte []
    offset      : uint64
    totalLength : uint32 }
