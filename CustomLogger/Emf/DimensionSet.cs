using System.Collections.Generic;

namespace CustomLogger.Emf;

/// <summary>
/// EMF DimensionSet.
/// </summary>
public class DimensionSet
{
    /// <summary>
    /// Set of dimension names.
    /// </summary>
    public ISet<string> Values { get; } = new HashSet<string>();
}
