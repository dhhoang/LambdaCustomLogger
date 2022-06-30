using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public interface ILogEntryFormatter
{
    public string Name { get; }

    string FormatToString<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> messageFormatter,
        IExternalScopeProvider scopeProvider);
}
