module ElectroElephant.SocketHelpers

open ElectroElephant.Response

open System
open System.IO
open System.Net.Sockets

type RecieveState =
   {  /// The socket which we read from.
      socket : Socket

      /// This buffer will contain the buffered data from the TCP socket.
      buffer : byte[]

      /// The size of the message, so we know when to stop reading.
      msg_size : int32

      /// To keep track of how far we've read.
      mutable total_read_bytes : int32

      /// a callback which we can send the deserialized object to
      callback : Response -> unit

      /// This stream is where we store the body of our response and will be given
      /// to the corresponding Response Serializer when we've read everything we need.
      stream : MemoryStream }

let begin_receive state remaining_bytes callback =
  state.socket.BeginReceive(
    state.buffer, 
    0, 
    remaining_bytes, 
    SocketFlags.None, 
    new AsyncCallback(callback), 
    state)

let end_receive state result =
  state.socket.EndReceive(result)

let begin_send socket =
  ()

let begin_end socket =
  ()

