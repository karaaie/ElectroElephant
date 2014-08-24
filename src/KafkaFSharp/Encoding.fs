module KafkaFsharp.Encoding

open KafkaFSharp.Model

open System

// MESSAGE SET: <MESSAGE LENGTH: uint32><MAGIC: 1 byte><COMPRESSION: 1 byte><CHECKSUM: uint32><MESSAGE PAYLOAD: bytes>
let encode_msg (msg : Message) : byte[] =
  let msg_length = NO_LEN_HEADER_SIZE + msg.payload.Length
  let msg_arr = Array.zeroCreate (msg_length + 4)
  let msg_len = (msg.totalLength |> BitConverter.GetBytes)
  Array.Copy(msg_len, 0, msg_arr, 0, 4)
  msg_arr.[4] <- msg.magic
  msg_arr.[5] <- msg.compression
  Array.Copy(msg.checksum, 0, msg_arr, 6, 4)
  Array.Copy(msg.payload, 0, msg_arr, 10, msg.payload.Length)
  msg_arr

//// MESSAGE SET: <MESSAGE LENGTH: uint32><MAGIC: 1 byte><COMPRESSION: 1 byte><CHECKSUM: uint32><MESSAGE PAYLOAD: bytes>
//func (m *Message) Encode() []byte {
//msgLen := NO_LEN_HEADER_SIZE + len(m.payload)
//msg := make([]byte, 4+msgLen)
//binary.BigEndian.PutUint32(msg[0:], uint32(msgLen))
//msg[4] = m.magic
//msg[5] = m.compression
//copy(msg[6:], m.checksum[0:])
//copy(msg[10:], m.payload)
//return msg
//}
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

