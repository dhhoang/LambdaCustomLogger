using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CustomLogger.Emf;

/// <summary>
/// Represents the scope of a set of EMF metric logs.
/// </summary>
public sealed class EmfMetricScope : IDisposable
{
    private class MetricDatum
    {
        public MetricDatum(string name, string unit, long value, IEnumerable<string> dimensions)
        {
            Name = name;
            Unit = unit;
            Value = value;
            Dimensions = dimensions.Take(9).ToArray();
        }

        public readonly string Name;
        public readonly string Unit;
        public readonly long Value;
        public readonly string[] Dimensions;
    }

    private int _disposed;
    private readonly Dictionary<string, string> _dimensions;
    private readonly string _metricNamespace;
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<MetricDatum> _metricQueue = new ConcurrentQueue<MetricDatum>();

    /// <summary>
    /// Initializes <see cref="EmfMetricScope"/>.
    /// </summary>
    /// <param name="metricNamespace">CloudWatch metric namespace.</param>
    /// <param name="dimensions">List of metric dimensions.</param>
    /// <param name="logger">Logger instance.</param>
    internal EmfMetricScope(
        string metricNamespace,
        IReadOnlyCollection<KeyValuePair<string, string>> dimensions,
        ILogger logger)
    {
        _dimensions = new Dictionary<string, string>(dimensions);
        _metricNamespace = metricNamespace;
        _logger = logger;
    }

    /// <summary>
    /// Records an EMF metric.
    /// </summary>
    /// <param name="name">Metric name.</param>
    /// <param name="unit">Metric unit.</param>
    /// <param name="value">Metric value.</param>
    /// <param name="dimensions">Metric dimensions.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the scope object has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if metric name or unit is invalid.</exception>
    public void PutMetric(string name, string unit, long value, IEnumerable<string> dimensions)
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) > 0)
        {
            throw new ObjectDisposedException(nameof(EmfMetricScope));
        }

        if (_dimensions.ContainsKey(name))
        {
            throw new ArgumentException(
                $"Cannot add metric '{name}' because a dimension with the same name already exists in the context",
                nameof(name));
        }

        _metricQueue.Enqueue(new MetricDatum(name, unit, value, dimensions));
        LogMetricIfNeeded(20, 20);
    }

    private void LogMetricIfNeeded(int batchSize, int stopThreshold)
    {
        while (_metricQueue.Count > stopThreshold)
        {
            var datumsToLog = new List<MetricDatum>(batchSize);
            for (var i = 0; i < batchSize; i++)
            {
                if (!_metricQueue.TryDequeue(out var d))
                {
                    break;
                }

                datumsToLog.Add(d);
            }

            DoLogMetrics(datumsToLog);
        }
    }

    private void DoLogMetrics(List<MetricDatum> metrics)
    {
        if (metrics.Count == 0)
        {
            return;
        }

        var metricMetadata = new MetricMetadata();
        var metricDirective = new MetricDirective(_metricNamespace);
        var emfData = new Dictionary<string, object>();

        foreach (var metricDatum in metrics)
        {
            // append the name-value of the metric to the EMF data block
            emfData[metricDatum.Name] = metricDatum.Value;

            // add the definition to reference the metric
            metricDirective.Metrics.Add(new MetricDefinition(metricDatum.Name, metricDatum.Unit));

            // add corresponding dimensions for the metrics
            var dimensionSet = new DimensionSet();
            foreach (var dimensionName in metricDatum.Dimensions)
            {
                // we only want to write the dimension that actually is initiated in the context
                if (!_dimensions.ContainsKey(dimensionName))
                {
                    continue;
                }

                dimensionSet.Values.Add(dimensionName);
                // also put that dimension to the emf data
                emfData.TryAdd(dimensionName, _dimensions[dimensionName]);
            }

            metricDirective.Dimensions.Add(dimensionSet);
        }

        metricMetadata.CloudWatchMetrics.Add(metricDirective);

        try
        {
            _logger.LogEmf(metricMetadata, emfData);
        }
        catch (OversizedLogException)
        {
            // if only 1 metric makes the record become oversized, stop trying
            if (metrics.Count < 2)
            {
                return;
            }

            // split the metrics in 2 halves and try to write each of them
            var halfIdx = metrics.Count / 2;
            DoLogMetrics(metrics.Take(halfIdx).ToList());
            DoLogMetrics(metrics.Skip(halfIdx).ToList());
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) > 0)
        {
            return;
        }

        LogMetricIfNeeded(20, 0);
    }
}
