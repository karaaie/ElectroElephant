module ElectroElephant.CompressionNew

open ElectroElephant.Common

// Kafka supports compressing messages for additional efficiency, 
// however this is more complex than just compressing a raw message. 
// Because individual messages may not have sufficient redundancy 
// to enable good compression ratios, compressed messages must 
// be sent in special batches (although you may use a batch of one 
// if you truly wish to compress a message on its own). The messages
// to be sent are wrapped (uncompressed) in a MessageSet structure, 
// which is then compressed and stored in the Value field of a 
// single "Message" with the appropriate compression codec set. 
// The receiving system parses the actual MessageSet from the decompressed value.
//   Message { 
//   Compression = Snappy, 
//   Value = MessageSet of N Messages, each message is uncompressed in this set.} 
type Compression =
  | None of MessageAttribute
  | GZIP of MessageAttribute
  | Snappy of MessageAttribute
