module ElectroElephant.Client

open ElectroElephant.Api
open ElectroElephant.Common
open ElectroElephant.MetadataResponse
open FSharp.Actor
open Logary
open System.Collections.Generic
open System.Net.Sockets

let logger = Logging.getCurrentLogger()

//Start and create the Actor System Host.
ActorHost.Start()
let system = ActorHost.CreateSystem "Electro Elephant Actor System"


type Broker = 
  { /// hostname of a broker
    hostname : Hostname
    /// the port of the broker
    port : Port }

type BootstrapConf = 
  { /// list of brokers that we can attempt to contact
    /// inorder to get metadata
    brokers : Broker list
    /// these are the topics that we want to constraint us to
    topics : TopicName list option }

type TopicPartition = 
  { topic : TopicName
    partition : PartitionId }

/// this is the object which is passed around and contains all the information we
/// need in order to publish messages.
type MetadataConfig =
  { topic_partition_map : Dictionary<TopicName, PartitionId list>
    topic_broker_map : Dictionary<TopicPartition, TcpClient> }

type SendAction =
  | Bootstrap of BootstrapConf
  | Send of byte []

let sender_actor (boot_conf : BootstrapConf) =
  actor {
    name "Sender Actor"
    messageHandler (fun actor ->
      let rec loop (meta_conf : MetadataConfig option) = async {
        match meta_conf with
        | Some cf ->
                  //do call the send method.
                  let head = boot_conf.brokers |> List.head
                  do_metadata_request head.hostname head.port boot_conf.topics
                  ()
        | None -> 
                  // do the bootstrap and send the conf as a state
                  //return! loop (Some conf)
                  return! loop None

        let! msg = actor.Receive()

        match msg.Message with
        | Send msg -> ()

        return! loop None
      }
      loop None
    )
  } |> system.SpawnActor

//sender_actor <-- SendAction.Bootstrap

/// <summary>
///   Calls the the list of kafka brokers until it gets a response
///   when it gets a response then it builds up a datastructure which
///   contains information about which kafka brokers are available and where
///   they are located.
/// </summary>
let start (conf : BootstrapConf) =
  (sender_actor conf) <-- SendAction.Bootstrap conf
