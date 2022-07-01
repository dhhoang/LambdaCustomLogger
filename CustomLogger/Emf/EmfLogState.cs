using System.Collections.Generic;

namespace CustomLogger.Emf;

internal class EmfLogState
{
    public EmfLogState(MetricMetadata metricMetadata, IReadOnlyDictionary<string, object> data)
    {
        MetricMetadata = metricMetadata;
        Data = data;
    }

    public MetricMetadata MetricMetadata { get; }
    public IReadOnlyDictionary<string, object> Data { get; }
}
