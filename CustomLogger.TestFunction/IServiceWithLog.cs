using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal interface IServiceWithLog
{
    void LogSingleLine();

    void LogMultiLine(int lineCount);

    void LogExceptionThrown();

    Task LogWithAsyncScopes(params object[] scopes);

    void LogEmfMetric();
}

internal class ServiceWithLog : IServiceWithLog
{
    private readonly ILogger<ServiceWithLog> _logger;

    private readonly Dictionary<string, string> _emfDimensions = new Dictionary<string, string>
    {
        ["functionVersion"] = "$LATEST",
        ["requestId"] = "989ffbf8-9ace-4817-a57c-e4dd734019ee"
    };

    public ServiceWithLog(ILogger<ServiceWithLog> logger) => _logger = logger;

    public void LogExceptionThrown()
    {
        try
        {
            _ = int.Parse("NotANumber", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception log from {ServiceName}", nameof(ServiceWithLog));
        }
    }

    public void LogMultiLine(int lineCount)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < lineCount; i++)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "Line {0} from ServiceWithLog", i).AppendLine();
        }

        _logger.LogInformation("{Message}", sb.ToString());
    }

    public void LogSingleLine() => _logger.LogWarning("Single line from {ServiceName}", nameof(ServiceWithLog));

    public async Task LogWithAsyncScopes(params object[] scopes)
    {
        await Task.Yield();
        var disposables = new IDisposable?[scopes.Length];

        try
        {
            for (var i = 0; i < scopes.Length; i++)
            {
                var scope = _logger.BeginScope(scopes[i]);
                disposables[i] = scope;
            }

            _logger.LogInformation("Log with scopes from {ServiceName}", nameof(ServiceWithLog));
        }
        finally
        {
            foreach (var d in disposables)
            {
                d?.Dispose();
            }
        }
    }

    public void LogEmfMetric()
    {
        using var emfScope = _logger.BeginEmf("custom-logger-metric", _emfDimensions);

        emfScope.PutMetric("time", "Milliseconds", 100, _emfDimensions.Keys);
        emfScope.PutMetric("time2", "Milliseconds", 200, _emfDimensions.Keys);
    }
}
