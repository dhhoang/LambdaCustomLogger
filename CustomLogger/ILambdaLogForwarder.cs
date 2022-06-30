using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal interface ILambdaLogForwarder
{
    void Forward(LogLevel logLevel, string entry);
}
