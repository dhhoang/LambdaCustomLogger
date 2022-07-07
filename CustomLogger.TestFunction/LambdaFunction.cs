using Amazon.Lambda.Core;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using System.IO;

//[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<CustomLogger.HttpApiJsonSerializerContext>))]

namespace CustomLogger.TestFunction;

//[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
//[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
//public partial class HttpApiJsonSerializerContext : JsonSerializerContext
//{
//}

public class LambdaFunction
{
    private readonly IServiceProvider _globalServices;

    public LambdaFunction()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        var services = new ServiceCollection();
        services
            .TryAddSingleton<IServiceWithLog, ServiceWithLog>();

        services.AddLogging(b => b.AddLambdaProxyLogger(opts =>
        {
            opts.HandlerName = LambdaLoggerOptions.JsonHandler;
        }));
        _globalServices = services.BuildServiceProvider();
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public async Task<Stream> Get(Stream request, ILambdaContext context)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        //LogUsingLambdaLogger(context.Logger);

        await using var requestServiceScope = _globalServices.CreateAsyncScope();
        var provider = requestServiceScope.ServiceProvider;

        var serviceWithLog = provider.GetRequiredService<IServiceWithLog>();
        serviceWithLog.LogSingleLine();
        serviceWithLog.LogMultiLine(3);

        await serviceWithLog.LogWithAsyncScopes(context.InvokedFunctionArn, context.AwsRequestId);
        serviceWithLog.LogExceptionThrown();

        serviceWithLog.LogEmfMetric();


        return Stream.Null;
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
            lambdaLogger.LogError("Exception log from Lambda " + ex);
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
