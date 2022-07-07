using System.Collections.Generic;
using CustomLogger.Emf;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public static class LoggerExtensions
{
    public static EmfMetricScope BeginEmf(
        this ILogger logger,
        string metricNamespace,
        IReadOnlyCollection<KeyValuePair<string, string>> dimensions) =>
        new EmfMetricScope(metricNamespace, dimensions, logger);
}
