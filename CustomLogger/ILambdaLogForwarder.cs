using System;

namespace CustomLogger;

public interface ILambdaLogForwarder
{
    void Forward(string entry);

    void Forward(ReadOnlySpan<byte> data);
}
