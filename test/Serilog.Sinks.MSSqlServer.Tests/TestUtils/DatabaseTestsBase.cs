using System;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    public abstract class DatabaseTestsBase : IDisposable
    {
        private bool _disposedValue;

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
