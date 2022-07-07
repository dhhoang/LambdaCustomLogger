using System;

namespace CustomLogger;

internal class Utf8ConsoleLambdaLogForwarder : ILambdaLogForwarder
{
    private readonly Action<string> _writeLineAction;

    internal Utf8ConsoleLambdaLogForwarder(Action<string> writeLineAction) => _writeLineAction = writeLineAction;

    public void Forward(string entry) => _writeLineAction(entry);

    public void Forward(ReadOnlySpan<byte> data) => _writeLineAction(LoggerHelper.Utf8NoBomNoThrow.GetString(data));
}
