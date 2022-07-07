using Microsoft.Extensions.Logging;
using System;

namespace CustomLogger;

internal class LambdaProxyLogger : ILogger
{
    private readonly IExternalScopeProvider? _scopeProvider;
    private readonly ILambdaLogForwarder _logForwarder;
    private readonly ILogHandler _handler;
    private readonly LogLevel _minLevel;

    public LambdaProxyLogger(ILambdaLogForwarder logForwarder, ILogHandler handler, IExternalScopeProvider? scopeProvider, LogLevel minLevel)
    {
        _logForwarder = logForwarder;
        _handler = handler;
        _minLevel = minLevel;
        _scopeProvider = scopeProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => _scopeProvider?.Push(state) ?? NoopDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= _minLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var entry = new LambdaLogEntry<TState>(logLevel, eventId, state, exception, formatter);
        _handler.Handle(entry, _logForwarder, _scopeProvider);
    }

    private class NoopDisposable : IDisposable
    {
        private NoopDisposable()
        {
        }

        public static NoopDisposable Instance { get; } = new NoopDisposable();

        public void Dispose()
        {
        }
    }
}
