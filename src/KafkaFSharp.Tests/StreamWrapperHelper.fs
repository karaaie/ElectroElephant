module ElectroElephant.Tests.StreamWrapperHelper

open System.IO

/// <summary>
///  Abstracts away the stream to stream mapping which is fiddly to do every single test
/// </summary>
/// <param name="input">data to serialize</param>
/// <param name="write_fun">the serialize function</param>
/// <param name="read_fun">the deserialize function</param>
let stream_wrapper<'Result> 
        (input : 'Result)
        (write_fun : 'Result -> MemoryStream -> unit) 
        (read_fun : MemoryStream -> 'Result) : 'Result =
    use write_stream = new MemoryStream()
    write_fun input write_stream
    write_stream.Flush()
    use read_stream = new MemoryStream(write_stream.ToArray())
    read_fun read_stream