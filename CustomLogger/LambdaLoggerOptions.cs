using Microsoft.Extensions.Options;

namespace CustomLogger;

internal class LambdaLoggerOptions : IOptions<LambdaLoggerOptions>
{
    public const string SimpleHandler = "Simple";

    public const string JsonHandler = "JSON";

    public LambdaLoggerOptions Value => this;

    public string HandlerName { get; set; } = SimpleHandler;
}
