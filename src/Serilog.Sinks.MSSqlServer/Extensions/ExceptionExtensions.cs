using System;
using System.Text;

namespace Serilog.Sinks.MSSqlServer.Extensions
{
    internal static class ExceptionExtensions
    {
        public static string ToMessageAndCompleteStackTrace(this Exception exception)
        {
            var s = new StringBuilder();
            while (exception != null)
            {
                s.AppendLine("Exception type: " + exception.GetType().FullName);
                s.AppendLine("Message: " + exception.Message);
                s.AppendLine();
                exception = exception.InnerException;
            }
            return s.ToString();
        }
    }
}
