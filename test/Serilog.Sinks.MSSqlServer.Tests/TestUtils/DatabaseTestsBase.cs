using System;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    [Collection("LogTest")]
    public abstract class DatabaseTestsBase : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private bool _disposedValue;

        public DatabaseTestsBase(ITestOutputHelper output)
        {
            _output = output;
            Serilog.Debugging.SelfLog.Enable(_output.WriteLine);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                DatabaseFixture.DropTable();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
