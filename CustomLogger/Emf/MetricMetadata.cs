using System;
using System.Collections.Generic;

namespace CustomLogger.Emf;

public class MetricMetadata
{
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    public List<MetricDirective> CloudWatchMetrics { get; } = new List<MetricDirective>();
}
