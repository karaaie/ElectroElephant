module ElectroElephant.Tests.Integration.Metadata

open ElectroElephant.Common
open ElectroElephant.MetadataResponse
open ElectroElephant.Client
open Fuchu

let broker_conf = 
  { brokers = 
      [ { hostname = "localhost"
          port = 9092 } ]
    topics = None }

/// This probably have to be adjusted from time to time.
let expected_metadata =
  { brokers = [ { node_id = 0
                  host = "192.168.1.48"
                  port = 9092}]
    topic_metadatas = [ { error_code = 0s
                          name = "yellowcars"
                          partitions = [ {  error_code = 0s
                                            id = 0
                                            leader = 0
                                            replicas = [0]
                                            isr = [0]}
                                            ]
                          } ]
  }


[<Tests>]
let tests = 
  testList "a series of integration tests" [ 
    testCase "attempt to get metadata" <| fun _ -> 
      let resp = bootstrap broker_conf |> Async.RunSynchronously
      Assert.Equal("should be equal", expected_metadata, resp)
  ]
