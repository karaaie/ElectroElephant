﻿module ElectroElephant.StreamHelpers

open ElectroElephant.Common
open Microsoft.FSharp.Core.Operators
open System
open System.IO
open System.Text

//===========Fixed Width Primitives==============
//
//int8, int16, int32, int64 
//   Signed integers with the given precision (in bits) stored 
//   in big endian order.
//
//=========Variable Length Primitives============
//
//bytes, string - These types consist of a signed integer 
// giving a length N followed by N bytes of content. 
// A length of -1 indicates null. string uses an int16 
// for its size, and bytes uses an int32.
//
//Arrays
//
//This is a notation for handling repeated structures. These will always be 
//encoded as an int32 size containing the length N followed by N repetitions of 
//the structure which can itself be made up of other primitive types. In the BNF 
//grammars below we will show an array of a structure foo as [foo].
let NullStringIndicator = -1s


/// This method ensures that everything we transmitt is in
/// big endian order and when we receive data from kafka we must
/// possibly convert it back to little endianess
let correct_endianess (bytes : byte[]) : byte [] =
  match BitConverter.IsLittleEndian with
  | true -> Array.rev bytes
  | false -> bytes


type MemoryStream with

  /// Reads a byte from the stream
  member this.read_byte() : int8 = 
    let bytes = Array.zeroCreate 1
    this.Read(bytes, 0, 1) |> ignore
    bytes |> (List.ofArray
              >> List.head
              >> int8)
  
  /// Writes a byte to the stream
  member this.write_byte (b : int8) = 
    let byte_arr = [ byte b ] |> Array.ofList
    this.Write(byte_arr, 0, 1)
  
  /// <summary>
  ///  Reads a int16 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int16<'IntType>() = 
    if sizeof<'IntType> <> sizeof<int16> then 
      failwithf "tried to read int16 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt16(bytes |> correct_endianess, 0)
  
  /// <summary>
  ///  Reads a int32 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int32<'IntType>() = 
    if sizeof<'IntType> <> sizeof<int32> then 
      failwithf "tried to read int32 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt32(bytes |> correct_endianess, 0)

  /// <summary>
  ///  Reads a int64 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int64<'IntType>() = 
    if sizeof<'IntType> <> sizeof<int64> then 
      failwithf "tried to read int32 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt64(bytes |> correct_endianess, 0)

  member this.read_int16_list<'ArrSize, 'IntType>() = 
    if sizeof<'IntType> <> sizeof<int16> then 
      failwithf "tried to read int16 list but typed it with %d byte int" sizeof<'IntType>
    let num_ints = this.read_int32<'ArrSize>()
    [ for i in 1..num_ints do
        yield this.read_int16<'IntType>() ]
  
  member this.read_int32_list<'ArrSize, 'IntType>() = 
    if sizeof<'IntType> <> sizeof<int32> then 
      failwithf "tried to read int32 list but typed it with %d byte int" sizeof<'IntType>
    let num_ints = this.read_int32<'ArrSize>()
    [ for i in 1..num_ints do
        yield this.read_int32<'IntType>() ]
  
  member this.read_int64_list<'ArrSize, 'IntType>() = 
    if sizeof<'IntType> <> sizeof<int64> then 
      failwithf "tried to read int64 list but typed it with %d byte int" sizeof<'IntType>
    let num_ints = this.read_int32<'ArrSize>()
    [ for i in 1..num_ints do
        yield this.read_int64<'IntType>() ]

  /// <summary>
  /// Writes a int16 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int16 to be written to stream</param>
  member this.write_int<'IntType> (i : int16) = 
    if sizeof<'IntType> <> sizeof<int16> then 
      failwithf "tried to write int16 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes |> correct_endianess, 0, sizeof<'IntType>)
  
  /// <summary>
  /// Writes a int32 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int32 to be written to stream</param>
  member this.write_int<'IntType> (i : int32) = 
    if sizeof<'IntType> <> sizeof<int32> then 
      failwithf "tried to write int32 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes |> correct_endianess, 0, sizeof<'IntType>)
  
  /// <summary>
  /// Writes a int64 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int64 to be written to stream</param>
  member this.write_int<'IntType> (i : int64) = 
    if sizeof<'IntType> <> sizeof<int64> then 
      failwithf "tried to write int64 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes |> correct_endianess, 0, sizeof<'IntType>)
  
  /// <summary>
  ///  Writes a list of int16 to the stream. It writes the size of the array
  ///  first, followed by each int16 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int16 to be written to stream</param>
  member this.write_int_list<'ArrSize, 'IntType> (int_list : int16 list) = 
    if sizeof<'IntType> <> sizeof<int16> then 
      failwithf "tried to write int16 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<'ArrSize> int_list.Length
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)
  
  /// <summary>
  ///  Writes a list of int32 to the stream. It writes the size of the array
  ///  first, followed by each int32 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int32 to be written to stream</param>
  member this.write_int_list<'ArrSize, 'IntType> (int_list : int32 list) = 
    if sizeof<'IntType> <> sizeof<int32> then 
      failwithf "tried to write int32 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<'ArrSize> int_list.Length
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)
  
  /// <summary>
  ///  Writes a list of int64 to the stream. It writes the size of the array
  ///  first, followed by each int16 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int64 to be written to stream</param>
  member this.write_int_list<'ArrSize, 'IntType> (int_list : int64 list) = 
    if sizeof<'IntType> <> sizeof<int64> then 
      failwithf "tried to write int64 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<'ArrSize> int_list.Length
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)
  
  /// <summary>
  /// reads a string from a stream. It will first read sizeof<StringSize> from
  /// from the stream to determine the size of the string. Then it will read
  /// and return the string as UTF8 encoded.
  /// </summary>
  /// <param name="stream">the stream to read from.</param>
  member this.read_str<'StrSize>() = 
    let str_size = this.read_int16<StringSize>()
    if str_size = NullStringIndicator then null
    else 
      let str_bytes = Array.zeroCreate (int str_size)
      this.Read(str_bytes, 0, int str_size) |> ignore
      Encoding.UTF8.GetString(str_bytes)
  
  /// <summary>
  ///  Reads and returns a list of of strings from the stream.
  /// </summary>
  member this.read_str_list<'ArrSize, 'StrSize>() = 
    let num_strs = this.read_int32<'ArrSize>()
    [ for i in 1..num_strs do
        yield this.read_str<'StrSize>() ]
  
  /// <summary>
  /// First writes the length of the string as an int to the stream
  /// then it writes the string UTF8 encoded
  /// </summary>
  /// <param name="str">the string to write to the stream</param>
  member this.write_str<'StrSize> (str : string) = 
    if str = null then this.write_int<StringSize> NullStringIndicator
    else 
      let name_bytes = Encoding.UTF8.GetBytes(str)
      this.write_int<'StrSize> (int16 name_bytes.Length)
      this.Write(Encoding.UTF8.GetBytes(str), 0, name_bytes.Length)
  
  /// <summary>
  ///  Writes the list of strings to the stream, it will first 
  ///  write the size of the array, then each string in order.
  /// </summary>
  /// <param name="str_list">string list to serialize</param>
  member this.write_str_list<'ArrSize, 'StrSize> (str_list : string list) = 
    this.write_int<'ArrSize> str_list.Length
    str_list |> List.iter (fun str -> this.write_str<'StrSize> str)

let read_array<'a> (stream : MemoryStream) (deserialize_fun : MemoryStream -> 'a) : 'a list = 
  let num = stream.read_int32<ArraySize>()
  [ for i in 1..num do
      yield deserialize_fun stream ]

let write_array<'a> (stream : MemoryStream) (items : 'a list) (serialize_fun : MemoryStream -> 'a -> unit) = 
  stream.write_int<ArraySize> items.Length
  items |> List.iter (serialize_fun stream)

let read_byte_array (stream : MemoryStream) = 
  let size = stream.read_int32<ByteArraySize>()
  let arr = Array.zeroCreate size
  stream.Read(arr, 0, size) |> ignore
  arr

let write_byte_array (stream : MemoryStream) (arr : byte []) = 
  stream.write_int<ByteArraySize> arr.Length
  stream.Write(arr, 0, arr.Length)