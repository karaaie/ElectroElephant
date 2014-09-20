module ElectroElephant.Client


open ElectroElephant.Api

open System.Net.Sockets

open ElectroElephant.MetadataResponse

open Logary

let logger = Logging.getCurrentLogger()

type Broker =
    /// hostname of a broker
  { hostname : string
    /// the port of the broker
    port : int}

type BootstrapConf =
  { /// list of brokers that we can attempt to contact
    /// inorder to get metadata
    brokers : Broker list
    /// these are the topics that we want to constraint us to
    topics : string list option}

/// <summary>
///   Transforms the metadata response to something we can reuse for later requests.
/// </summary>
/// <param name="meta"></param>
let private populate_kafka_metadata (meta : MetadataResponse) =
  LogLine.Create "MetaResponse arrived! This is the content"
  |> Log.setData "metadata" meta
  |> logger.Log
 
/// <summary>
///   Calls the the list of kafka brokers until it gets a response
///   when it gets a response then it builds up a datastructure which
///   contains information about which kafka brokers are available and where
///   they are located.
/// </summary>
let bootstrap ( conf : BootstrapConf) =
  let head = conf.brokers |> List.head
  do_metadata_request head.hostname head.port conf.topics populate_kafka_metadata
