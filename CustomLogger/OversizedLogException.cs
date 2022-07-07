using System;

namespace CustomLogger;

/// <summary>
/// Indicates that a log entry exceeds CloudWatch Logs size limit.
/// </summary>
public class OversizedLogException : InvalidOperationException
{
    /// <summary>
    /// Initializes an instance of <see cref="OversizedLogException"/>.
    /// </summary>
    /// <param name="size">Entry size.</param>
    public OversizedLogException(int size) : base($"Log entry size exceeds the limit: {size}") => Size = size;

    /// <summary>
    /// Size of log entry.
    /// </summary>
    public int Size { get; }
}
