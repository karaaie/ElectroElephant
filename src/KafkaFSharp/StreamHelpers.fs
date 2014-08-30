module ElectroElephant.StreamHelpers

open System
open System.IO
open System.Text

open Microsoft.FSharp.Core.Operators

open ElectroElephant.Common

type IntReturnType =
 | Bytes2 of int16
 | Bytes4 of int32
 | Bytes8 of int64


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
  member this.read_int () : int16 =
    let size = sizeof<int16>
    let bytes = Array.zeroCreate sizeof<int16>
    let read_bytes = this.Read(bytes, 0, size)
    if size <> read_bytes then
      failwithf """failed to read integer from size, 
        read %d bytes but expected %d""" read_bytes size 
    BitConverter.ToInt16(bytes, 0)

  /// <summary>
  ///  Reads a int16 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int () : int32 =
    let size = sizeof<int16>
    let bytes = Array.zeroCreate sizeof<int16>
    let read_bytes = this.Read(bytes, 0, size)
    if size <> read_bytes then
      failwithf """failed to read integer from size, 
        read %d bytes but expected %d""" read_bytes size 
    BitConverter.ToInt32(bytes, 0)
  

  /// <summary>
  ///  Reads a int16 from the stream
  /// </summary>
  /// <param name="stream">the stream to read from</param>
  member this.read_int () : int64 =
    let size = sizeof<int64>
    let bytes = Array.zeroCreate sizeof<int64>
    let read_bytes = this.Read(bytes, 0, size)
    if size <> read_bytes then
      failwithf """failed to read integer from size, 
        read %d bytes but expected %d""" read_bytes size 
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
  member this.read_str () =
    let str_size : StringSize = this.read_int ()
    let str_bytes = Array.zeroCreate (int str_size)
    this.Read(str_bytes, 0, int str_size) |> ignore
    Encoding.UTF8.GetString(str_bytes)

  /// <summary>
  /// First writes the length of the string as an int to the stream
  /// then it writes the string UTF8 encoded
  /// </summary>
  /// <param name="str">the string to write to the stream</param>
  member this.write_str<'StrSize> (str : string) =
    this.Write(str.Length |> BitConverter.GetBytes, 0, sizeof<'StrSize>)
    let name_bytes = Encoding.UTF8.GetBytes(str)
    this.Write(name_bytes, 0, name_bytes.Length)

  /// <summary>
  ///  Writes the list of strings to the stream, it will first 
  ///  write the size of the array, then each string in order.
  /// </summary>
  /// <param name="str_list"></param>
  member this.write_str_list<'ArrSize, 'StrSize> (str_list : string list) =
    this.write_int<'ArrSize> str_list.Length
    str_list |> List.iter (fun str ->
      this.write_int<'StrSize> str.Length
      this.write_str str )
