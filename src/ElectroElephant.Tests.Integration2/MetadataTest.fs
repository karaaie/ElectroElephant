module ElectroElephant.Tests.Integration.Metadata

open ElectroElephant.Client
open ElectroElephant.MetadataResponse
open Fuchu

open System
open System.Threading

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
      [ { hostname = "192.168.1.89"
          port = 9092 } ]
    topics = None }


/// IMPORTANT, if your running these tests against a local cluster then make sure that
/// the cluster is started, and that you have created a atleast one topic. For some reason
/// the kafka broker will claim it has no brokers until a topic is created.
[<Tests>]
let tests = 
  testList "smoke tests" [
    testCase "attempt to get metadata and verify we get something sane back." <| fun _ -> 
      let reset_event = new ManualResetEvent(false)

      let callback (resp : MetadataResponse) =
        Assert.Equal("should have one broker", 1, resp.brokers.Length)
        Assert.Equal("should have one topic", 1, resp.topic_metadatas.Length)
        Assert.Equal("should contain a yellow car topic", true ,
          resp.topic_metadatas |> List.exists (fun t -> t.name.Contains "yellocars"))
        reset_event.Set() |> ignore

      Async.RunSynchronously (bootstrap broker_conf callback)
      reset_event.WaitOne() |> ignore
   ]