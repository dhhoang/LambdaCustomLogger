using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public class JsonLogEntryHandler : ILogHandler
{
    public string Name => LambdaLoggerOptions.JsonHandler;

    public void Handle<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider scopeProvider)
    {
        var output = new ArrayBufferWriter<byte>(1024);
        using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
        {
            Indented = false
        });

        writer.WriteStartObject();

        // write formatted message
        writer.WriteString("_formattedMessage", entry.Formatter(entry.State, entry.Exception));

        // write scopes 
        writer.WriteStartArray("_scopes");

        scopeProvider.ForEachScope(
            (o, w) => w.WriteStringValue(ToInvariantString(o)),
            writer);

        writer.WriteEndArray();

        // write exception
        if (entry.Exception is Exception entryException)
        {
            writer.WriteString(nameof(Exception), entryException.ToString());
        }

        // write state properties
        if (entry.State is IReadOnlyCollection<KeyValuePair<string, object>> logProperties)
        {
            foreach (var s in logProperties)
            {
                WriteItem(writer, s);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        forwarder.Forward(output.WrittenMemory.Span);
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
