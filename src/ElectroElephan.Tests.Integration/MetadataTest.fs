module ElectroElephant.Tests.Integration.Metadata

open ElectroElephant.Client
open Fuchu

//type Broker =
//    /// hostname of a broker
//  { hostname : string
//    /// the port of the broker
//    port : int}
//
//type BootstrapConf =
//  { /// list of brokers that we can attempt to contact
//    /// inorder to get metadata
//    brokers : Broker list
//    /// these are the topics that we want to constraint us to
//    topics : string list option}
let broker_conf = 
  { brokers = 
      [ { hostname = "localhost"
          port = 9092 } ]
    topics = None }

[<Tests>]
let tests = testList "" [ testCase "attempt to get metadata" <| fun _ -> bootstrap broker_conf |> ignore ]
