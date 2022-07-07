using System.Collections.Generic;

namespace CustomLogger.Emf;

public class DimensionSet
{
    public ISet<string> Values { get; } = new HashSet<string>();
}
