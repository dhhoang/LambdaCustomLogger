using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace CustomLogger;

/// <summary>
/// Provides method for adding Lambda log provider.
/// </summary>
public static class AWSLambdaLoggerFactoryExtensions
{
    private const string EnvironmentVariableTelemetryLogFd = "_LAMBDA_TELEMETRY_LOG_FD";

    /// <summary>
    /// Adds the Lambda Log provider to the builder.
    /// </summary>
    /// <param name="builder">The extension method argument.</param>
    /// <param name="configureOptions">Configure logging options.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddLambdaProxyLogger(this ILoggingBuilder builder, Action<LambdaLoggerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions();
        builder.Services.Configure(configureOptions);

        builder.Services.AddSingleton<ILogHandler, SimpleLogHandler>();
        builder.Services.AddSingleton<ILogHandler, JsonLogEntryHandler>();

        builder.Services.AddSingleton<ILambdaLogForwarder>(_ =>
        {
            try
            {
                return new TelemetryFdLogFowarder(int.Parse(Environment.GetEnvironmentVariable(EnvironmentVariableTelemetryLogFd) ?? string.Empty,
                    CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fallback to stdout due to error {0}", ex);
                return new Utf8ConsoleLambdaLogForwarder(Console.Out.WriteLine);
            }
        });
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LambdaLoggerProvider>());

        return builder;
    }
}
