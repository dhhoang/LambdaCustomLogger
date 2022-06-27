using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AmznCore = Amazon.Lambda.Core;

namespace CustomLogger;

internal class LambdaProxyLogger : ILogger
{
    private static readonly LogLevel EnvironmentMinLevel = Enum.TryParse<AmznCore.LogLevel>(Environment.GetEnvironmentVariable("AWS_LAMBDA_HANDLER_LOG_LEVEL"), out var minLevel)
        ? AmznToMsLogLevel(minLevel) : LogLevel.Information;

    private readonly AmznCore.ILambdaLogger _lambdaLogger;
    private readonly LogLevel _minLevel;

    public LambdaProxyLogger(AmznCore.ILambdaLogger lambdaLogger) : this(lambdaLogger, EnvironmentMinLevel)
    {
    }

    public LambdaProxyLogger(AmznCore.ILambdaLogger lambdaLogger, LogLevel minLevel)
    {
        _lambdaLogger = lambdaLogger;
        _minLevel = minLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= _minLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (state is IReadOnlyCollection<KeyValuePair<string, object>> structure)
        {
            var output = new ArrayBufferWriter<byte>(1024);
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
            {
                Indented = false
            });

            writer.WriteStartObject();
            foreach (var s in structure)
            {
                WriteItem(writer, s);
            }
            writer.WriteEndObject();

            writer.Flush();
            _lambdaLogger.Log(MsToAmznLogLevel(logLevel), Encoding.UTF8.GetString(output.WrittenMemory.Span));
            return;
        }

        var msg = formatter(state, exception);
        _lambdaLogger.Log(MsToAmznLogLevel(logLevel), msg);
    }

    private static void WriteItem(Utf8JsonWriter writer, KeyValuePair<string, object> item)
    {
        var key = item.Key;
        switch (item.Value)
        {
            case bool boolValue:
                writer.WriteBoolean(key, boolValue);
                break;
            case byte byteValue:
                writer.WriteNumber(key, byteValue);
                break;
            case sbyte sbyteValue:
                writer.WriteNumber(key, sbyteValue);
                break;
            case char charValue:
                writer.WriteString(key, MemoryMarshal.CreateSpan(ref charValue, 1));
                break;
            case decimal decimalValue:
                writer.WriteNumber(key, decimalValue);
                break;
            case double doubleValue:
                writer.WriteNumber(key, doubleValue);
                break;
            case float floatValue:
                writer.WriteNumber(key, floatValue);
                break;
            case int intValue:
                writer.WriteNumber(key, intValue);
                break;
            case uint uintValue:
                writer.WriteNumber(key, uintValue);
                break;
            case long longValue:
                writer.WriteNumber(key, longValue);
                break;
            case ulong ulongValue:
                writer.WriteNumber(key, ulongValue);
                break;
            case short shortValue:
                writer.WriteNumber(key, shortValue);
                break;
            case ushort ushortValue:
                writer.WriteNumber(key, ushortValue);
                break;
            case null:
                writer.WriteNull(key);
                break;
            default:
                writer.WriteString(key, ToInvariantString(item.Value));
                break;
        }
    }

    private static string? ToInvariantString(object? obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

    private static AmznCore.LogLevel MsToAmznLogLevel(LogLevel msLogLevel) => msLogLevel switch
    {
        LogLevel.Trace => AmznCore.LogLevel.Trace,
        LogLevel.Debug => AmznCore.LogLevel.Debug,
        LogLevel.Information => AmznCore.LogLevel.Information,
        LogLevel.Warning => AmznCore.LogLevel.Warning,
        LogLevel.Error => AmznCore.LogLevel.Error,
        LogLevel.Critical => AmznCore.LogLevel.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(msLogLevel))
    };

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
