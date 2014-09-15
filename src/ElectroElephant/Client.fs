module ElectroElephant.Client

open System.Net.Sockets

type Broker =
  { hostname : string
    port : int}


type BootstrapConf =
  { brokers : Broker list
    topic : string list option}


/// <summary>
///   Calls the the list of kafka brokers until it gets a response
///   when it gets a response then it builds up a datastructure which
///   contains information about which kafka brokers are available and where
///   they are located.
/// </summary>
let bootstrap ( conf : BootstrapConf) =
  conf.brokers
  |> List.map (fun brk -> new TcpClient(brk.hostname, brk.port))

  // Call Api with conf,
  // await response
  // build up a datastructure to be able to map topic-partition to a broker.
  
  

