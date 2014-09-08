module ElectroElephant.Tests.Request

open ElectroElephant.Request
open ElectroElephant.Tests.StreamWrapperHelper
open FsCheck
open Fuchu

let tests = 
  testList "" [ testCase "Serialize and deserialize should be the same" <| fun _ -> 
                  let same (req : Request) = 
                    req = stream_wrapper<Request> req serialize deserialize
                  Check.QuickThrowOnFailure same ]
