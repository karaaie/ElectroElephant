module OffsetCommitResponseTests


open Fuchu

[<Tests>]
let tests =
  testList "" [
    testCase "Serialize and Deserialize should be the same" <| fun _ ->
      Assert.Equal("equal", true,false)
  ]