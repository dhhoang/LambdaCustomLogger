using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal class SimpleLogEntryFormatter : ILogEntryFormatter
{
    public string Name => LambdaLoggerOptions.SimpleFormatter;

    public string FormatToString<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> messageFormatter, IExternalScopeProvider scopeProvider)
    {
        var scopeBuilder = new StringBuilder();
        scopeProvider.ForEachScope(
            (o, s) =>
            {
                Console.WriteLine("scope {0}", o);
                s.Append('<').Append(o).Append('>');
            },
            scopeBuilder);

        var scope = scopeBuilder.ToString();
        var msg = messageFormatter(state, exception);
        return $"[{logLevel}] {scope} {msg} {exception}";
    }
}
