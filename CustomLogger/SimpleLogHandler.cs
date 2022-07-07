using System.Text;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal class SimpleLogHandler : ILogHandler
{
    public string Name => LambdaLoggerOptions.SimpleHandler;

    public void Handle<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider? scopeProvider)
    {
        var scopeBuilder = new StringBuilder();
        scopeProvider?.ForEachScope(
            (o, s) =>
            {
                s.Append('<').Append(o).Append('>');
            },
            scopeBuilder);

        var scope = scopeBuilder.ToString();
        var formattedMessage = entry.Formatter(entry.State, entry.Exception);

        forwarder.Forward($"[{entry.Level}] {scope} {formattedMessage} {entry.Exception}");
    }
}
