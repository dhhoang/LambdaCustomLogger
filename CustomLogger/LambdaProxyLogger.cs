using Microsoft.Extensions.Logging;
using System;
using AmznCore = Amazon.Lambda.Core;

namespace CustomLogger;

internal class LambdaProxyLogger : ILogger
{
    public static readonly LogLevel EnvironmentMinLevel =
        Enum.TryParse<AmznCore.LogLevel>(Environment.GetEnvironmentVariable("AWS_LAMBDA_HANDLER_LOG_LEVEL"), out var minLevel)
            ? AmznToMsLogLevel(minLevel)
            : LogLevel.Information;

    private readonly IExternalScopeProvider _scopeProvider;
    private readonly ILambdaLogForwarder _logForwarder;
    private readonly ILogHandler _handler;
    private readonly LogLevel _minLevel;

    public LambdaProxyLogger(ILambdaLogForwarder logForwarder, ILogHandler handler, IExternalScopeProvider scopeProvider, LogLevel minLevel)
    {
        _logForwarder = logForwarder;
        _handler = handler;
        _minLevel = minLevel;
        _scopeProvider = scopeProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);

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

    // private static AmznCore.LogLevel MsToAmznLogLevel(LogLevel msLogLevel) => msLogLevel switch
    // {
    //     LogLevel.Trace => AmznCore.LogLevel.Trace,
    //     LogLevel.Debug => AmznCore.LogLevel.Debug,
    //     LogLevel.Information => AmznCore.LogLevel.Information,
    //     LogLevel.Warning => AmznCore.LogLevel.Warning,
    //     LogLevel.Error => AmznCore.LogLevel.Error,
    //     LogLevel.Critical => AmznCore.LogLevel.Critical,
    //     _ => throw new ArgumentOutOfRangeException(nameof(msLogLevel))
    // };

    private static LogLevel AmznToMsLogLevel(AmznCore.LogLevel amznLogLevel) => amznLogLevel switch
    {
        AmznCore.LogLevel.Trace => LogLevel.Trace,
        AmznCore.LogLevel.Debug => LogLevel.Debug,
        AmznCore.LogLevel.Information => LogLevel.Information,
        AmznCore.LogLevel.Warning => LogLevel.Warning,
        AmznCore.LogLevel.Error => LogLevel.Error,
        AmznCore.LogLevel.Critical => LogLevel.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(amznLogLevel))
    };
}
