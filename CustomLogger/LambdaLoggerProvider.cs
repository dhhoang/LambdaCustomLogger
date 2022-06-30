using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomLogger;

internal class LambdaLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ILambdaLogForwarder _logForwarder;
    private readonly ILogHandler _formatter;
    private IExternalScopeProvider _scopeProvider = NoneExternalScopeProvider.Instance;

    public LambdaLoggerProvider(ILambdaLogForwarder logFowarder, IEnumerable<ILogHandler> formatters, IOptions<LambdaLoggerOptions> options)
    {
        _logForwarder = logFowarder;
        _formatter = formatters.FirstOrDefault(f => f.Name == options.Value.HandlerName)
                     ?? throw new ArgumentException($"Cannot find formatter with name {options.Value.HandlerName}", nameof(formatters));
    }

    public ILogger CreateLogger(string categoryName) =>
        new LambdaProxyLogger(_logForwarder, _formatter, _scopeProvider, LambdaProxyLogger.EnvironmentMinLevel);

    public void Dispose() { }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;
}
