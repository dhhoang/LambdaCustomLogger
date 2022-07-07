using System.Collections.Generic;
using CustomLogger.Emf;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

/// <summary>
/// Logger extensions.
/// </summary>
public static class LoggerEmfExtensions
{
    /// <summary>
    /// Begins a scope for a EMF logging context.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="metricNamespace">CloudWatch metric namespace.</param>
    /// <param name="dimensions">Dimension data.</param>
    /// <returns>A new EMF logging scope.</returns>
    public static EmfMetricScope BeginEmf(
        this ILogger logger,
        string metricNamespace,
        IReadOnlyCollection<KeyValuePair<string, string>> dimensions) =>
        new EmfMetricScope(metricNamespace, dimensions, logger);
}
