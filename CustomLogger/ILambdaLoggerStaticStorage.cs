using Amazon.Lambda.Core;

namespace CustomLogger;

internal interface ILambdaLoggerStaticStorage
{
    void SetLogger(ILambdaLogger logger);

    ILambdaLogger? GetLogger();
}
