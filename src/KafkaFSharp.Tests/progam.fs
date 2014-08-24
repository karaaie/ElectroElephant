module KafkaFSharp.Tests

open System
open Fuchu

[<EntryPoint>]
let main args =
  let r = defaultMainThisAssembly args
  Console.ReadLine() |> ignore
  r