module ElectroElephant.StreamHelpers

open System
open System.IO
open System.Text

open Microsoft.FSharp.Core.Operators

open ElectroElephant.Common


type MemoryStream with

// Blargh... WHY U NO ?!?!?!
// Covariants / Contravariant.. must look in to it.
//  member this.read_int<'IntType> () =
//    let size = sizeof<'IntType>
//    let bytes = Array.zeroCreate sizeof<'IntType>
//    let read_bytes = this.Read(bytes, 0, size)
//    if size <> read_bytes then
//      failwithf """failed to read integer from size, 
//        read %d bytes but expected %d""" read_bytes size 
//    match size with
//    | 2 -> BitConverter.ToInt16(bytes, 0)
//    | 4 -> BitConverter.ToInt32(bytes, 0)
//    | 8 -> BitConverter.ToInt64(bytes, 0)

  /// <summary>
  ///  Reads a int16 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int16<'IntType> () =
    if sizeof<'IntType> <> sizeof<int16> then
      failwithf "tried to read int16 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt16(bytes, 0)

  /// <summary>
  ///  Reads a int32 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int32<'IntType> () =
    if sizeof<'IntType> <> sizeof<int32> then
      failwithf "tried to read int32 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt32(bytes, 0)

  /// <summary>
  ///  Reads a int64 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int64<'IntType> () =
    if sizeof<'IntType> <> sizeof<int64> then
      failwithf "tried to read int32 but typed it with %d byte int" sizeof<'IntType>
    let size = sizeof<'IntType>
    let bytes = Array.zeroCreate sizeof<'IntType>
    this.Read(bytes, 0, size) |> ignore
    BitConverter.ToInt64(bytes, 0)

  /// <summary>
  /// Writes a int16 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int16 to be written to stream</param>
  member this.write_int<'IntType> ( i : int16 ) =
    if sizeof<'IntType> <> sizeof<int16> then
      failwithf "tried to write int16 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes, 0, sizeof<'IntType>)

  /// <summary>
  /// Writes a int32 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int32 to be written to stream</param>
  member this.write_int<'IntType> ( i : int32 ) =
    if sizeof<'IntType> <> sizeof<int32> then
      failwithf "tried to write int32 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes, 0, sizeof<'IntType>)

  /// <summary>
  /// Writes a int64 to the stream, the given type will be compared
  /// to int16, if they don't match an error will be thrown.
  /// </summary>
  /// <param name="i">int64 to be written to stream</param>
  member this.write_int<'IntType> ( i : int64 )=
    if sizeof<'IntType> <> sizeof<int64> then
      failwithf "tried to write int64 but typed it with %d byte int" sizeof<'IntType>
    this.Write(i |> BitConverter.GetBytes, 0, sizeof<'IntType>)

  /// <summary>
  ///  Writes a list of int16 to the stream. It writes the size of the array
  ///  first, followed by each int16 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int16 to be written to stream</param>
  member this.write_int_list<'ByteArrSize, 'IntType> ( int_list : int16 list) =
    if sizeof<'IntType> <> sizeof<int16> then
      failwithf "tried to write int16 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<'ByteArrSize> (int_list.Length * sizeof<'IntType>)
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)

  /// <summary>
  ///  Writes a list of int32 to the stream. It writes the size of the array
  ///  first, followed by each int32 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int32 to be written to stream</param>
  member this.write_int_list<'ByteArrSize, 'IntType> ( int_list : int32 list) =
    if sizeof<'IntType> <> sizeof<int32> then
      failwithf "tried to write int32 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<'ByteArrSize> (int_list.Length * sizeof<'IntType>)
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)

  /// <summary>
  ///  Writes a list of int64 to the stream. It writes the size of the array
  ///  first, followed by each int16 in order they are provided.
  /// </summary>
  /// <param name="int_list">list of int64 to be written to stream</param>
  member this.write_int_list<'ByteArrSize, 'IntType> ( int_list : int64 list) =
    if sizeof<'IntType> <> sizeof<int64> then
      failwithf "tried to write int64 but typed it with %d byte int" sizeof<'IntType>
    this.write_int<ByteArraySize> (int_list.Length * sizeof<'IntType>)
    int_list |> List.iter (fun i -> this.write_int<'IntType> i)

  /// <summary>
  /// reads a string from a stream. It will first read sizeof<StringSize> from
  /// from the stream to determine the size of the string. Then it will read
  /// and return the string as UTF8 encoded.
  /// </summary>
  /// <param name="stream">the stream to read from.</param>
  member this.read_str<'StrSize> () =
    let str_size = this.read_int16<StringSize> ()
    if str_size = -1s then
      null
    else
      let str_bytes = Array.zeroCreate (int str_size)
      this.Read(str_bytes, 0, int str_size) |> ignore
      Encoding.UTF8.GetString(str_bytes)

  /// <summary>
  ///  Reads and returns a list of of strings from the stream.
  /// </summary>
  member this.read_str_list<'ArrSize, 'StrSize> () =
    let num_strs = this.read_int32<'ArrSize> ()
    [ for i in 1..num_strs do yield this.read_str<'StrSize> () ]

  /// <summary>
  /// First writes the length of the string as an int to the stream
  /// then it writes the string UTF8 encoded
  /// </summary>
  /// <param name="str">the string to write to the stream</param>
  member this.write_str<'StrSize> (str : string) =
    if str = null then
      this.write_int<StringSize> -1s
    else
      let name_bytes = Encoding.UTF8.GetBytes(str)
      this.write_int<'StrSize> (int16 name_bytes.Length)
      this.Write(Encoding.UTF8.GetBytes(str), 0, name_bytes.Length)

  /// <summary>
  ///  Writes the list of strings to the stream, it will first 
  ///  write the size of the array, then each string in order.
  /// </summary>
  /// <param name="str_list"></param>
  member this.write_str_list<'ArrSize, 'StrSize> (str_list : string list) =
    this.write_int<'ArrSize> str_list.Length
    str_list |> List.iter (fun str -> this.write_str<'StrSize> str)
