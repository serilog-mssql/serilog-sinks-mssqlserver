using System;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class TestableSqlCommandExecutor : SqlCommandExecutor
    {
        public TestableSqlCommandExecutor(
            ISqlWriter sqlWriter,
            ISqlConnectionFactory sqlConnectionFactory) : base(sqlWriter, sqlConnectionFactory)
        {
        }

        public Action<Exception> HandleExceptionCallback { get; set; }

        protected override void HandleException(Exception ex)
        {
            HandleExceptionCallback?.Invoke(ex);
        }
    }
}
