using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using CustomLogger.Emf;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public class JsonLogEntryHandler : ILogHandler
{
    public string Name => LambdaLoggerOptions.JsonHandler;

    public void Handle<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider scopeProvider)
    {
        switch (entry.State)
        {
            case EmfLogState emfLogState:
                LogEmf(emfLogState, forwarder);
                return;
            default:
                LogStructured(entry, forwarder, scopeProvider);
                break;
        }
    }

    private static void LogStructured<TState>(in LambdaLogEntry<TState> entry, ILambdaLogForwarder forwarder, IExternalScopeProvider scopeProvider)
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

    private static void LogEmf(EmfLogState state, ILambdaLogForwarder forwarder)
    {
        var output = new ArrayBufferWriter<byte>(1024);
        using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
        {
            Indented = false
        });

        writer.WriteStartObject();

        writer.WriteStartObject("_aws");

        writer.WriteNumber("Timestamp", state.MetricMetadata.Timestamp.ToUnixTimeMilliseconds());

        writer.WriteStartArray("CloudWatchMetrics");

        foreach (var metricDirective in state.MetricMetadata.CloudWatchMetrics)
        {
            writer.WriteStartObject();

            writer.WriteString("Namespace", metricDirective.Namespace);

            writer.WriteStartArray("Dimensions");
            foreach (var dimensionSet in metricDirective.Dimensions)
            {
                writer.WriteStartArray();
                foreach (var dimension in dimensionSet.Values)
                {
                    writer.WriteStringValue(dimension);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray(); // Dimensions

            writer.WriteStartArray("Metrics");
            foreach (var metricDefinition in metricDirective.Metrics)
            {
                writer.WriteStartObject();
                writer.WriteString("Name", metricDefinition.Name);
                writer.WriteString("Unit", metricDefinition.Unit);
                writer.WriteEndObject();
            }

            writer.WriteEndArray(); // Metrics

            writer.WriteEndObject();
        }

        writer.WriteEndArray(); // CloudWatchMetrics

        writer.WriteEndObject(); // _aws

        // write the state data
        foreach (var stateKvp in state.Data)
        {
            WriteItem(writer, stateKvp);
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
