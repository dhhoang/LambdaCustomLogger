using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace CustomLogger;

internal static class AWSLambdaLoggerFactoryExtensions
{
    private const string EnvironmentVariableTelemetryLogFd = "_LAMBDA_TELEMETRY_LOG_FD";

    /// <summary>
    /// Adds an event logger named 'EventLog' to the factory.
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

        builder.Services.AddSingleton<ILambdaLogForwarder>(sp =>
        {
            try
            {
                return new TelemetryFdLogFowarder(int.Parse(Environment.GetEnvironmentVariable(EnvironmentVariableTelemetryLogFd) ?? string.Empty,
                    CultureInfo.InvariantCulture));
            }
            catch
            {
                return new Utf8StdoutLambdaLogForwarder();
            }
        });
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LambdaLoggerProvider>());

        return builder;
    }
}
