module ElectroElephant.StreamHelpers

open System
open System.IO
open System.Text

open Microsoft.FSharp.Core.Operators

open ElectroElephant.Common


type MemoryStream with

// Blargh... WHY U NO ?!?!?!
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

  member private this.write_int16 i =
    this.Write((int16 i) |> BitConverter.GetBytes, 0, sizeof<int16>)

  member private this.write_int32 i =
    this.Write((int32 i) |> BitConverter.GetBytes, 0, sizeof<int32>)

  member private this.write_int64 i =
    this.Write((int64 i) |> BitConverter.GetBytes, 0, sizeof<int64>)

  /// <summary>
  /// Writes an integer to the stream, the type parameter will
  /// decide how many bytes the int consists of.
  /// </summary>
  /// <param name="i">the integer to write to the stream</param>
  member this.write_int<'IntType> i =
    // This is a bit hacky, but the generic system did not allow me to
    // to write a fully generic write_int method. BitConverter.GetBytes
    // didn't understand which overload to use.
    match sizeof<'IntType> with
    | 2       -> i |> this.write_int16
    | 4       -> i |> this.write_int32
    | 8       -> i |> this.write_int64
    | unknown -> failwithf "non-supported int with size %d" unknown

  /// <summary>
  /// reads a string from a stream. It will first read sizeof<StringSize> from
  /// from the stream to determine the size of the string. Then it will read
  /// and return the string as UTF8 encoded.
  /// </summary>
  /// <param name="stream">the stream to read from.</param>
  member this.read_str<'StrSize> () =
    let str_size = this.read_int16<StringSize> ()
    let str_bytes = Array.zeroCreate (int str_size)
    this.Read(str_bytes, 0, int str_size) |> ignore
    Encoding.UTF8.GetString(str_bytes)

  member this.read_str_list<'ArrSize, 'StrSize> () =
    let num_strs = this.read_int32<'ArrSize> ()
    [ for i in 1..num_strs do yield this.read_str<'StrSize> () ]

  /// <summary>
  /// First writes the length of the string as an int to the stream
  /// then it writes the string UTF8 encoded
  /// </summary>
  /// <param name="str">the string to write to the stream</param>
  member this.write_str<'StrSize> (str : string) =
    let name_bytes = Encoding.UTF8.GetBytes(str)
    this.write_int<'StrSize> name_bytes.Length
    this.Write(Encoding.UTF8.GetBytes(str), 0, name_bytes.Length)

  /// <summary>
  ///  Writes the list of strings to the stream, it will first 
  ///  write the size of the array, then each string in order.
  /// </summary>
  /// <param name="str_list"></param>
  member this.write_str_list<'ArrSize, 'StrSize> (str_list : string list) =
    this.write_int<'ArrSize> str_list.Length
    str_list |> List.iter (fun str -> this.write_str<'StrSize> str)
