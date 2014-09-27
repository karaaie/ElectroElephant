module KafkaFSharp.Integration.Tests

open Fuchu
open Logary
open Logary.Targets
open Logary.Rule
open Logary.Configuration

[<EntryPoint>]
let main args = 
  use logary = 
    withLogary' "ElectroElephant" 
      (withTargets [ Console.create Console.empty "console"
                     Debugger.create Debugger.DebuggerConf.Default "debugger" ]
       >> withRules [ Rule.createForTarget "console"
                      Rule.createForTarget "debugger" ])
  defaultMainThisAssembly args