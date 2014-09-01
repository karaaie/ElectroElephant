module ElectroElephant.Message

open ElectroElephant.Common
open ElectroElephant.CompressionNew

type Message =
    // The CRC is the CRC32 of the remainder of the message
    // bytes. This is used to check the integrity of 
    // the message on the broker and consumer.
  { crc         : Crc32

    // This is a version id used to allow backwards compatible evolution of the message binary format.
    magic_byte  : MagicByte

    // This byte holds metadata attributes about the
    //  message. The lowest 2 bits contain the compression codec 
    // used for the message. The other bits should be set to 0.
    attributes  : Compression

    // The key is an optional message key that was 
    // used for partition assignment. The key can be null.
    key         : MessageKey

    // The value is the actual message contents as an opaque 
    // byte array. Kafka supports recursive messages in 
    // which case this may itself contain a 
    // message set. The message can be null.
    value       : MessageValue }

type MessageSet =
  //This is the offset used in kafka as the log sequence number. 
  // When the producer is sending messages it doesn't actually 
  // know the offset and can fill in any value here it likes.
  { offset    : Offset
    messages  : Message list }

open System.IO
open ElectroElephant.StreamHelpers

let serialize message_set (stream : MemoryStream) : unit =  ()

let deserialize (stream : MemoryStream) =
  { offset = 0L
    messages = [] }
