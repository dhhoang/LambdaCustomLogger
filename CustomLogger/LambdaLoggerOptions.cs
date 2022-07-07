using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomLogger;

/// <summary>
/// Options for the lambda logger.
/// </summary>
public class LambdaLoggerOptions : IOptions<LambdaLoggerOptions>
{
    /// <summary>
    /// Simple log handler.
    /// </summary>
    public const string SimpleHandler = "Simple";

    /// <summary>
    /// Json log handler.
    /// </summary>
    public const string JsonHandler = "JSON";

    /// <inheritdoc/>
    public LambdaLoggerOptions Value => this;

    /// <summary>
    /// Name of the log handler.
    /// </summary>
    public string HandlerName { get; set; } = SimpleHandler;

    /// <summary>
    /// Minimum log level.
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Information;
}
