using Microsoft.Extensions.Options;

namespace CustomLogger;

internal class LambdaLoggerOptions : IOptions<LambdaLoggerOptions>
{
    public const string SimpleFormatter = "Simple";

    public const string JsonFormatter = "JSON";

    public LambdaLoggerOptions Value => this;

    public string FormatterName { get; set; } = SimpleFormatter;
}
