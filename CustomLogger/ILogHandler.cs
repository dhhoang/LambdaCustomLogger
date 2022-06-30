using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public interface ILogHandler
{
    public string Name { get; }

    void Handle<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider scopeProvider);
}
