using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CustomLogger.Emf;

public static class LambdaLoggerEmfExtension
{
    public static void LogEmf(this ILogger logger, MetricMetadata metricMetadata, IReadOnlyDictionary<string, object> data)
        => logger.Log(LogLevel.Information, 0, new EmfLogState(metricMetadata, data), null, EmfMessageFormatter);

    private static string EmfMessageFormatter(EmfLogState state, Exception? exception) => string.Empty;
}
