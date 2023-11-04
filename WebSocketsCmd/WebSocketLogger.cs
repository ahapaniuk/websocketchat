using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSockets.Common;
using System.Diagnostics;

namespace WebSocketsCmd
{
    internal class WebSocketLogger : IWebSocketLogger
    {
        // TODO: починить Trace (конфигурация?)
        public void Information(Type type, string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
            //Trace.TraceInformation(format, args);
        }

        public void Warning(Type type, string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
            //Trace.TraceWarning(format, args);
        }

        public void Error(Type type, string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
            //Trace.TraceError(format, args);
        }

        public void Error(Type type, Exception exception)
        {
            Console.WriteLine(exception.Message);
            //Error(type, "{0}", exception);
        }
    }
}
