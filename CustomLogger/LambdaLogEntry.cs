using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public readonly struct LambdaLogEntry<TState>
{
    public LogLevel Level { get; }
    public EventId EventId { get; }
    public TState State { get; }
    public Exception? Exception { get; }
    public Func<TState, Exception?, string> Formatter { get; }

    public LambdaLogEntry(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Level = logLevel;
        EventId = eventId;
        State = state;
        Exception = exception;
        Formatter = formatter;
    }
}
