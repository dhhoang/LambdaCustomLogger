using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Interface for handling logs message.
/// </summary>
public interface ILogHandler
{
    /// <summary>
    /// Name of the handler.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Handles a log entry.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="entry"></param>
    /// <param name="forwarder"></param>
    /// <param name="scopeProvider"></param>
    void Handle<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider? scopeProvider);
}
