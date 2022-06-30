using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public class JsonLogEntryFormatter : ILogEntryFormatter
{
    public string Name => LambdaLoggerOptions.JsonFormatter;

    public string FormatToString<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> messageFormatter, IExternalScopeProvider scopeProvider)
    {
        var msg = messageFormatter(state, exception);

        if (state is IReadOnlyCollection<KeyValuePair<string, object>> logProperties)
        {
            var output = new ArrayBufferWriter<byte>(1024);
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
            {
                Indented = false
            });

            writer.WriteStartObject();

            writer.WriteString("_formattedMessage", msg);

            // write scopes 
            writer.WriteStartArray("_scopes");

            scopeProvider.ForEachScope(
                (o, w) => w.WriteStringValue(ToInvariantString(o)),
                writer);

            writer.WriteEndArray();

            if (exception is not null)
            {
                writer.WriteString(nameof(Exception), exception.ToString());
            }

            foreach (var s in logProperties)
            {
                WriteItem(writer, s);
            }

            writer.WriteEndObject();

            writer.Flush();
            return Encoding.UTF8.GetString(output.WrittenMemory.Span);
        }

        return JsonSerializer.Serialize(msg);
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
}
