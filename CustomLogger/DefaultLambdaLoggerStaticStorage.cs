using System.Threading;
using Amazon.Lambda.Core;

namespace CustomLogger;

internal class DefaultLambdaLoggerStaticStorage : ILambdaLoggerStaticStorage
{
    private readonly AsyncLocal<ILambdaLogger> _lambdaLoggerStorage = new AsyncLocal<ILambdaLogger>();

    public ILambdaLogger? GetLogger() => _lambdaLoggerStorage.Value;
    public void SetLogger(ILambdaLogger logger) => _lambdaLoggerStorage.Value = logger;
}
