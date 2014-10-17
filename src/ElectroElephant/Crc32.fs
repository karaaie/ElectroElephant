module ElectroElephant.Crc32

open ElectroElephant.Common


/// TODO: Untested...
module private Internal =
  let crcTable = 
      let inline nextValue acc =
          if 0 <> (acc &&& 1) then 0xedb88320 ^^^ (acc >>> 1) else acc >>> 1
      let rec iter k acc =
          if k = 0 then acc else iter (k-1) (nextValue acc)
      [| 0 .. 255 |] |> Array.map (iter 8)

/// Returns the CRC-32 value of 'name' as specified by RFC1952
let crc32 (arr : byte []) : Crc32 =
  let inline update acc (ch:byte) =
      Internal.crcTable.[int32 ((acc ^^^ (int32 ch)) &&& 0xff)] ^^^ (acc >>> 8)

  0xFFffFFff ^^^ Seq.fold update 0xFFffFFff arr