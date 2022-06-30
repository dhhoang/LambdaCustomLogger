Custom log provider for Lambda that conforms to `Microsoft.Extensions.Logging.ILogger` that can be used with .NET DI.
See `LambdaFunction.cs` file for example usage.

### Custom log format/handler
You can write your own Log handler that transforms logs to any format by implementing `ILogHandler`. See `JsonLogEntryHandler` for example. Register it in the DI pipeline:

```csharp
Services.AddSingleton<ILogHandler, JsonLogEntryHandler>();
```

Then in the log configuration tell the DI to use your handler
```csharp
services.AddLogging(b => b.AddLambdaProxyLogger(opts =>
{
    opts.HandlerName = "MyCustomLogHandler";
}));
```
