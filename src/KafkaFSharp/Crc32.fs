﻿module FsharpKafka.Crc32


 module private Internal =
     let crcTable = 
         let inline nextValue acc =
             if 0u <> (acc &&& 1u) then 0xedb88320u ^^^ (acc >>> 1) else acc >>> 1
         let rec iter k acc =
             if k = 0 then acc else iter (k-1) (nextValue acc)
         [| 0u .. 255u |] |> Array.map (iter 8)
 
 
 /// Returns the CRC-32 value of 'name' as specified by RFC1952
 let crc32 (name:string) =
     let inline update acc (ch:char) =
         Internal.crcTable.[int32 ((acc ^^^ (uint32 ch)) &&& 0xffu)] ^^^ (acc >>> 8)
     0xFFffFFffu ^^^ Seq.fold update 0xFFffFFffu name