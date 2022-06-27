//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CustomLogger;

//internal static class AWSLambdaLoggerFactoryExtensions
//{
//    /// <summary>
//    /// Adds an event logger named 'EventLog' to the factory.
//    /// </summary>
//    /// <param name="builder">The extension method argument.</param>
//    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
//    public static ILoggingBuilder AddEventLog(this ILoggingBuilder builder)
//    {
//        ArgumentNullException.ThrowIfNull(builder);

//        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LambdaLoggerProvider>());

//        return builder;
//    }
//}
