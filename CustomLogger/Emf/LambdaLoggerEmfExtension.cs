using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CustomLogger.Emf;

/// <summary>
/// Provides helper method for logging an metric in EMF format.
/// </summary>
public static class LambdaLoggerEmfExtension
{
    /// <summary>
    /// Logs an EMF metric that can be picked up by CloudWatch.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="metricMetadata">MetricMetadata object.</param>
    /// <param name="data">Set of key-value pairs containing metric data.</param>
    public static void LogEmf(this ILogger logger, MetricMetadata metricMetadata, IReadOnlyDictionary<string, object> data)
        => logger.Log(LogLevel.Information, 0, new EmfLogState(metricMetadata, data), null, EmfMessageFormatter);

    private static string EmfMessageFormatter(EmfLogState state, Exception? exception) => string.Empty;
}
