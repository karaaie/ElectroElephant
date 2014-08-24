module KafkaFSharp.encoding_tests

open Fuchu
open System
open System.Text

open KafkaFSharp.Generator
open KafkaFSharp.Compression
open KafkaFSharp.Encoding

let original_string : string = "looooong text with mucho importante stuff in it,
  gargerglls garaasdflkasdf; asdflaksdfasdfmn mmcmcmcjsjhadjfa sdfasld;kfjakls;df
  asdflaksdf;kasd;lfksadf , , as;ldfkasdfk;asdf,,,, vmmmvnkfvlf
"
let msg_bytes = Encoding.UTF8.GetBytes(original_string)

let original_message = create_new_message msg_bytes GZIP_COMPRESSION

let encoded_message = encode_msg original_message
let decoded_message = decode_msg encoded_message

[<Tests>]
let tests =
  testList "" [
    testCase "encoded message should have payload size as first bytes 4 bytes" <| fun _ ->
      let expected_size = original_message.payload_size |> BitConverter.GetBytes
      Assert.Equal(
        "",
        expected_size,
        encoded_message.[0..3])

    testCase "encoded message should have magic byte set" <| fun _ ->
      Assert.Equal(
        "",
        original_message.magic,
        encoded_message.[MAGIC_BYTE_INDEX])

    testCase "encoded message should have compression correctly set" <| fun _ ->
      Assert.Equal(
        "",
        original_message.compression,
        encoded_message.[COMPRESSION_BYTE_INDEX])

    testCase "encoded message should have identical checksum" <| fun _ ->
      Assert.Equal(
        "",
        original_message.checksum,
        encoded_message.[CHECKSUM_START_BYTE_INDEX..(CHECKSUM_START_BYTE_INDEX+3)])

    testCase "encoded message should have identical payload" <| fun _ ->
      Assert.Equal(
        "",
        original_message.payload,
        encoded_message.[PAYLOAD_START_BYTE_INDEX..])

    testCase "decoded message should have identical payload size" <| fun _ ->
      let encoded_payload_size = BitConverter.ToUInt32(encoded_message,0)
      Assert.Equal(
        "",
        encoded_payload_size,
        decoded_message.payload_size)

    testCase "decoded message should have identical magic byte" <| fun _ ->
      Assert.Equal(
        "",
        encoded_message.[MAGIC_BYTE_INDEX],
        decoded_message.magic)

    testCase "decoded message should have identical compression" <| fun _ ->
      Assert.Equal(
        "",
        encoded_message.[COMPRESSION_BYTE_INDEX],
        decoded_message.compression)

    testCase "decoded message should have identical checksum" <| fun _ ->
      Assert.Equal(
        "",
        encoded_message.[CHECKSUM_START_BYTE_INDEX..(CHECKSUM_START_BYTE_INDEX+3)],
        decoded_message.checksum)

    testCase "decoded message should have identical payloads" <| fun _ ->
      Assert.Equal(
        "",
        original_message.payload,
        decoded_message.payload)

    testCase "decoded message should contain original message" <| fun _ ->
      let decompressed_msg = get_payload decoded_message
      Assert.Equal(
        "",
        original_string,
        decompressed_msg |> Encoding.UTF8.GetString)
  ]








