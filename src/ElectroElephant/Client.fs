module ElectroElephant.Client

open ElectroElephant.Api
open ElectroElephant.MetadataResponse
open Logary
open Logary.Configuration
open Logary.Rule
open Logary.Targets
open System.Net.Sockets

let logger = Logging.getCurrentLogger()

type Broker = 
  { /// hostname of a broker
    hostname : string
    /// the port of the broker
    port : int }

type BootstrapConf = 
  { /// list of brokers that we can attempt to contact
    /// inorder to get metadata
    brokers : Broker list
    /// these are the topics that we want to constraint us to
    topics : string list option }

///// <summary>
/////   Transforms the metadata response to something we can reuse for later requests.
///// </summary>
///// <param name="meta"></param>
//let private populate_kafka_metadata (meta : MetadataResponse) = 
//  LogLine.info "MetaResponse arrived! This is the content"
//  |> LogLine.setData "metadata" (sprintf "%A" meta)
//  |> Logger.log logger

/// <summary>
///   Calls the the list of kafka brokers until it gets a response
///   when it gets a response then it builds up a datastructure which
///   contains information about which kafka brokers are available and where
///   they are located.
/// </summary>
let bootstrap (conf : BootstrapConf) : Async<MetadataResponse> =
  let head = conf.brokers |> List.head
  do_metadata_request head.hostname head.port conf.topics
