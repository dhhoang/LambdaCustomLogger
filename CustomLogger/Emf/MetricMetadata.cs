using System;
using System.Collections.Generic;

namespace CustomLogger.Emf;

/// <summary>
/// EMF MetricMetadata object.
/// </summary>
public class MetricMetadata
{
    /// <summary>
    /// Metric timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// List of <see cref="MetricDirective"/>.
    /// </summary>
    public List<MetricDirective> CloudWatchMetrics { get; } = new List<MetricDirective>();
}
