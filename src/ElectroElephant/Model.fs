module ElectroElephant.Model

open ElectroElephant.Common
open ElectroElephant.Response
open System.IO
open System.Net.Sockets

type RecieveState = 
  { /// The socket which we read from.
    socket : Socket
    /// This buffer will contain the buffered data from the TCP socket.
    buffer : byte []
    /// The size of the message, so we know when to stop reading.
    msg_size : MessageSize
    /// To keep track of how far we've read.
    mutable total_read_bytes : int32
    /// a callback which we can send the deserialized object to
    callback : Response -> unit
    /// the correlation_id set when sending the request to the server
    corr_id : CorrelationId
    /// This stream is where we store the body of our response and will be given
    /// to the corresponding Response Serializer when we've read everything we need.
    stream : MemoryStream }
