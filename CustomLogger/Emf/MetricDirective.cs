using System.Collections.Generic;

namespace CustomLogger.Emf;

public class MetricDirective
{
    public MetricDirective(string @namespace) => Namespace = @namespace;

    public string Namespace { get; }

    public List<DimensionSet> Dimensions { get; } = new List<DimensionSet>();

    public List<MetricDefinition> Metrics { get; } = new List<MetricDefinition>();
}
