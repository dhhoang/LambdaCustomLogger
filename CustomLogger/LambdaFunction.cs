using System.Net;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Text;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomLogger.HttpApiJsonSerializerContext>))]

namespace CustomLogger;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}

public class LambdaFunction
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("Sample.CustomLogger", "1.0.0");

    private readonly AsyncLocal<ILambdaLogger> _lambdaLoggerStorage = new AsyncLocal<ILambdaLogger>();
    private readonly IServiceProvider _globalServices;

    public LambdaFunction()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        var services = new ServiceCollection();
        services
            .TryAddSingleton<IAmazonS3, AmazonS3Client>();
        services
            .TryAddSingleton<IS3MetaGetter, S3MetaGetter>();

        services.AddLogging(b => b.AddLambdaProxyLogger(opts =>
        {
            opts.FormatterName = LambdaLoggerOptions.JsonFormatter;
        }));
        _globalServices = services.BuildServiceProvider();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> Get(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        _lambdaLoggerStorage.Value = context.Logger;
        //context.Logger.LogInformation(GetMultiLineRecord());

        await using var requestServiceScope = _globalServices.CreateAsyncScope();
        var provider = requestServiceScope.ServiceProvider;

        var lambdaLoggerMsg = GetMultiLineRecord("Lambda");
        context.Logger.LogInformation(lambdaLoggerMsg);

        //SetLoggerScope(provider, context);
        var logger = provider.GetRequiredService<ILogger<LambdaFunction>>();

        using var lambdaScope = logger.BeginScope(context.InvokedFunctionArn);
        using var requestScope = logger.BeginScope(context.AwsRequestId);

        logger.LogInformation(GetMultiLineRecord("Huy"));

        try
        {
            _ = int.Parse("NotANumber", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test exception log for request {RequestId}", context.AwsRequestId);
        }

        var response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "Hello AWS Serverless",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }

    private static string GetMultiLineRecord(string source)
    {
        var sb = new StringBuilder();
        sb
            .Append("this is line 1 from ").Append(source).AppendLine()
            .Append("this is line 2 from ").Append(source).AppendLine()
            .Append("this is line 2 from ").Append(source);

        return sb.ToString();
    }

    private static void JustThrow()
    {
        JustThrow2();
    }

    private static void JustThrow2()
    {
        JustThrow3();
    }

    private static void JustThrow3()
    {
        throw new InvalidOperationException("trying to raise");
    }

    private static void SetLoggerScope(IServiceProvider sp, ILambdaContext lambdaContext)
    {
        var staticStorage = sp.GetRequiredService<ILambdaLoggerStaticStorage>();
        staticStorage.SetLogger(lambdaContext.Logger);
    }
}
