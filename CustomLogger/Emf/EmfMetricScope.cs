using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CustomLogger.Emf;

public class EmfMetricScope : IDisposable
{
    private struct MetricDatum
    {
        public readonly string Unit;
        public readonly long Value;
        public readonly string[] Dimensions;

        public MetricDatum(string unit, long value, IReadOnlySet<string> dimensions)
        {
            Unit = unit;
            Value = value;
            Dimensions = dimensions.ToArray();
        }
    }

    private volatile bool _disposed;
    private readonly ILogger _logger;
    private readonly MetricDirective _metricDirective;
    private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

    private readonly ConcurrentDictionary<string, MetricDatum> _datums = new ConcurrentDictionary<string, MetricDatum>();

    public EmfMetricScope(string metricNamespace, IReadOnlyDictionary<string, string> dimensions, ILogger logger)
    {
        _metricDirective = new MetricDirective(metricNamespace);
        _logger = logger;
        foreach (var dimension in dimensions)
        {
            _data[dimension.Key] = dimension.Value;
        }
    }

    public void PutMetric(string name, string unit, long value, IReadOnlySet<string> dimensions)
    {
        foreach (var dimension in dimensions)
        {
            if (!_data.ContainsKey(dimension))
            {
                throw new ArgumentException($"Dimension name '{dimension}' cannot be found", nameof(dimensions));
            }
        }

        _datums[name] = new MetricDatum(unit, value, dimensions);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var metricMetadata = new MetricMetadata();

        foreach (var metricDatum in _datums)
        {
            _data[metricDatum.Key] = metricDatum.Value.Value;
            _metricDirective.Metrics.Add(new MetricDefinition(metricDatum.Key, metricDatum.Value.Unit));
            var ds = new DimensionSet();
            foreach (var s in metricDatum.Value.Dimensions)
            {
                ds.Values.Add(s);
            }

            _metricDirective.Dimensions.Add(ds);
        }

        metricMetadata.CloudWatchMetrics.Add(_metricDirective);
        _logger.LogEmf(metricMetadata, _data);
    }
}
