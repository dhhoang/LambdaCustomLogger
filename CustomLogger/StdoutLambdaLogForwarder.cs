using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal class StdoutLambdaLogForwarder : ILambdaLogForwarder
{
    public void Forward(LogLevel logLevel, string entry)
    {
        var writer = logLevel >= LogLevel.Error ? Console.Error : Console.Out;
        writer.WriteLine(entry);
    }
}
