using System;
using System.Text;

namespace CustomLogger;

internal class Utf8StdoutLambdaLogForwarder : ILambdaLogForwarder
{
    public void Forward(string entry) => Console.Out.WriteLine(entry);
    public void Forward(ReadOnlySpan<byte> data)
    {
        try
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }
        catch
        {
            // ignore
        }
    }
}
