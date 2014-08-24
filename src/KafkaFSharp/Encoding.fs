module KafkaFSharp.Encoding

open KafkaFSharp.Model
open KafkaFSharp.Compression
open KafkaFSharp.Crc32

open System

// Compression Support uses '1' - https://cwiki.apache.org/confluence/display/KAFKA/Compression
let MAGIC_CONST = 1

// magic + compression + chksum
let NO_LEN_HEADER_SIZE = MAGIC_CONST + 1 + 4

let MESSAGE_LENGTH_BYTE_INDEX = 0
let MAGIC_BYTE_INDEX = 4
let COMPRESSION_BYTE_INDEX = 5
let CHECKSUM_BYTE_INDEX = 6
let PAYLOAD_BYTE_INDEX = 10

// MESSAGE SET: <MESSAGE LENGTH: uint32><MAGIC: 1 byte><COMPRESSION: 1 byte><CHECKSUM: uint32><MESSAGE PAYLOAD: bytes>
let encode_msg (msg : Message) : byte[] =
  let msg_length = NO_LEN_HEADER_SIZE + msg.payload.Length
  let msg_arr = Array.zeroCreate (msg_length + 4)
  let msg_len = (msg.payload_size |> BitConverter.GetBytes)
  Array.Copy(msg_len, 0, msg_arr, MESSAGE_LENGTH_BYTE_INDEX, 4)
  msg_arr.[MAGIC_BYTE_INDEX] <- msg.magic
  msg_arr.[COMPRESSION_BYTE_INDEX] <- msg.compression
  Array.Copy(msg.checksum, 0, msg_arr, CHECKSUM_BYTE_INDEX, 4)
  Array.Copy(msg.payload, 0, msg_arr, PAYLOAD_BYTE_INDEX, msg.payload.Length)
  msg_arr

let decode_msg (decoded_msg : byte[]) : Message =
  let compression_type = compression_from_id decoded_msg.[COMPRESSION_BYTE_INDEX]

  //make sure we actually got all our data
  let payload = decoded_msg.[10..]
  let provided_payload_size = BitConverter.ToUInt32(decoded_msg, 0)
  if provided_payload_size <> uint32 payload.Length then
    failwith "Incorrect package size, might have lost data"

  //make sure the data is not corrupt
  let crc32_provided = BitConverter.ToUInt32(decoded_msg, CHECKSUM_BYTE_INDEX)
  let crc32_actual = crc32 payload
  if crc32_provided <> crc32_actual then
    failwithf "checksum fail, data is possibly corrupt, expected: %d but got %d" crc32_actual crc32_provided

  let decompressed = decompress_data compression_type decoded_msg.[10..]

  { magic = decoded_msg.[MAGIC_BYTE_INDEX]
    compression = decoded_msg.[COMPRESSION_BYTE_INDEX]
    payload = decompressed
    checksum = Array.sub decoded_msg CHECKSUM_BYTE_INDEX 4
    offset = 0UL
    payload_size = BitConverter.ToUInt32(decoded_msg, 0) }
  
  
//func DecodeWithDefaultCodecs(packet []byte) (uint32, []Message) {
//return Decode(packet, DefaultCodecsMap)
//}
//func Decode(packet []byte, payloadCodecsMap map[byte]PayloadCodec) (uint32, []Message) {
//messages := []Message{}
//length, message := decodeMessage(packet, payloadCodecsMap)
//if length > 0 && message != nil {
//if message.compression != NO_COMPRESSION_ID {
//// wonky special case for compressed messages having embedded messages
//payloadLen := uint32(len(message.payload))
//messageLenLeft := payloadLen
//for messageLenLeft > 0 {
//start := payloadLen - messageLenLeft
//innerLen, innerMsg := decodeMessage(message.payload[start:], payloadCodecsMap)
//messageLenLeft = messageLenLeft - innerLen - 4 // message length uint32
//messages = append(messages, *innerMsg)
//}
//} else {
//messages = append(messages, *message)
//}
//}
//return length, messages
//}
//func decodeMessage(packet []byte, payloadCodecsMap map[byte]PayloadCodec) (uint32, *Message) {
//if len(packet) < 5 {
//log.Printf("malformed packet with length:%d (%#v), skipping\n", len(packet), packet)
//return 0, nil
//}
//length := binary.BigEndian.Uint32(packet[0:])
//if length > uint32(len(packet[4:])) {
//log.Printf("length mismatch, expected at least: %X, was: %X\n", length, len(packet[4:]))
//return 0, nil
//}
//msg := Message{}
//msg.totalLength = length
//msg.magic = packet[4]
//rawPayload := []byte{}
//if msg.magic == 0 {
//msg.compression = byte(0)
//copy(msg.checksum[:], packet[5:9])
//payloadLength := length - 1 - 4
//rawPayload = packet[9 : 9+payloadLength]
//} else if msg.magic == MAGIC_DEFAULT {
//msg.compression = packet[5]
//copy(msg.checksum[:], packet[6:10])
//payloadLength := length - NO_LEN_HEADER_SIZE
//rawPayload = packet[10 : 10+payloadLength]
//} else {
//log.Printf("incorrect magic, expected: %X was: %X\n", MAGIC_DEFAULT, msg.magic)
//return 0, nil
//}
//payloadChecksum := make([]byte, 4)
//binary.BigEndian.PutUint32(payloadChecksum, crc32.ChecksumIEEE(rawPayload))
//if !bytes.Equal(payloadChecksum, msg.checksum[:]) {
//msg.Print()
//log.Printf("checksum mismatch, expected: % X was: % X\n", payloadChecksum, msg.checksum[:])
//return 0, nil
//}
//msg.payload = payloadCodecsMap[msg.compression].Decode(rawPayload)
//return length, &msg
//}

