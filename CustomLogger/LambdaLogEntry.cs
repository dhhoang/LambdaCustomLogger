using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Represents a log entry.
/// </summary>
/// <typeparam name="TState"></typeparam>
public readonly struct LambdaLogEntry<TState>
{
    /// <summary>
    /// Log level.
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// Event ID.
    /// </summary>
    public EventId EventId { get; }

    /// <summary>
    /// Log entry state.
    /// </summary>
    public TState State { get; }

    /// <summary>
    /// Exception attached to the entry.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Function to get the formatted log message.
    /// </summary>
    public Func<TState, Exception?, string> Formatter { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="LambdaLogEntry{TState}"/>.
    /// </summary>
    /// <param name="logLevel">Log level.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="state">Log entry state.</param>
    /// <param name="exception">Exception attached to the entry.</param>
    /// <param name="formatter">Function to get the formatted log message.</param>
    public LambdaLogEntry(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Level = logLevel;
        EventId = eventId;
        State = state;
        Exception = exception;
        Formatter = formatter;
    }
}
