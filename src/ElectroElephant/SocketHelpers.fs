module ElectroElephant.SocketHelpers

open ElectroElephant.Common
open ElectroElephant.Model
open ElectroElephant.Response
open System
open System.IO
open System.Net.Sockets

/// <summary>
///   This method inits a begin receive for the given socket
/// </summary>
/// <param name="state"></param>
/// <param name="remaining_bytes"></param>
/// <param name="callback"></param>
let begin_receive state remaining_bytes callback = 
  state.socket.BeginReceive
    (state.buffer, 0, remaining_bytes, SocketFlags.None, 
     new AsyncCallback(callback), state) /// lets do the error handling inside the callback.
                                         |> ignore

let end_receive state result = state.socket.EndReceive(result)

let begin_send socket = ()
let begin_end socket = ()