using System.Net;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Text;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomLogger.HttpApiJsonSerializerContext>))]

namespace CustomLogger;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}

public class LambdaFunction
{
    private readonly AsyncLocal<ILambdaLogger> _lambdaLoggerStorage = new AsyncLocal<ILambdaLogger>();
    private readonly IServiceProvider _globalServices;

    public LambdaFunction()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        var services = new ServiceCollection();
        services
            .TryAddSingleton<IServiceWithLog, ServiceWithLog>();

        services.AddLogging(b => b.AddLambdaProxyLogger(opts =>
        {
            opts.HandlerName = LambdaLoggerOptions.SimpleHandler;
        }));
        _globalServices = services.BuildServiceProvider();
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public async Task<APIGatewayHttpApiV2ProxyResponse> Get(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _lambdaLoggerStorage.Value = context.Logger;

        LogUsingLambdaLogger(context.Logger);

        await using var requestServiceScope = _globalServices.CreateAsyncScope();
        var provider = requestServiceScope.ServiceProvider;

        var logger = provider.GetRequiredService<ILogger<LambdaFunction>>();

        var serviceWithLog = provider.GetRequiredService<IServiceWithLog>();
        serviceWithLog.LogSingleLine();
        serviceWithLog.LogMultiLine(3);

        await serviceWithLog.LogWithAsyncScopes(context.InvokedFunctionArn, context.AwsRequestId);
        serviceWithLog.LogExceptionThrown();

        var response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "Hello AWS Serverless",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }

    private static void LogUsingLambdaLogger(ILambdaLogger lambdaLogger)
    {
        lambdaLogger.LogInformation(GetMultiLineRecord("Lambda", 3));

        try
        {
            _ = int.Parse("NotANumber", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            lambdaLogger.LogError("Exception log from Lambda " + ex.ToString());
        }
    }

    private static string GetMultiLineRecord(string source, int lineCount)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < lineCount; i++)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "Line {0} from {1}", i, source).AppendLine();
        }

        return sb.ToString();
    }
}
