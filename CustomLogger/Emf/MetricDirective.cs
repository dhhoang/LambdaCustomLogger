using System.Collections.Generic;

namespace CustomLogger.Emf;

/// <summary>
/// EMF MetricDirective object.
/// </summary>
public class MetricDirective
{
    /// <summary>
    /// Initializes a new instance of <see cref="MetricDirective"/>.
    /// </summary>
    /// <param name="namespace"></param>
    public MetricDirective(string @namespace) => Namespace = @namespace;

    /// <summary>
    /// CloudWatch metric namespace.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Metric dimension set.
    /// </summary>
    public List<DimensionSet> Dimensions { get; } = new List<DimensionSet>();

    /// <summary>
    /// Metrics definition.
    /// </summary>
    public List<MetricDefinition> Metrics { get; } = new List<MetricDefinition>();
}
