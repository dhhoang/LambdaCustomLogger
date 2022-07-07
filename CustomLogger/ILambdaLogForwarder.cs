using System;

namespace CustomLogger;

/// <summary>
/// Allow forwarding log entries to Lambda logging output.
/// </summary>
public interface ILambdaLogForwarder
{
    /// <summary>
    /// Forwards a log entry as a string.
    /// </summary>
    /// <param name="entry">Log entry.</param>
    void Forward(string entry);

    /// <summary>
    /// Forwards a log entry as byte buffer.
    /// </summary>
    /// <param name="data"></param>
    void Forward(ReadOnlySpan<byte> data);
}
