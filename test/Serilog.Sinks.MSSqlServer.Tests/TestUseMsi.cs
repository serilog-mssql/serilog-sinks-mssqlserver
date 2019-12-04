using System;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class TestUseMsi : IDisposable
    {
        [Fact]
        public void TestIfUseMsiTrueAndAzureServiceTokenProviderResourceNotSet_ThrowsArgumentNullException()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            Assert.Throws<ArgumentNullException>(() => loggerConfiguration.AuditTo.MSSqlServer(
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    autoCreateSqlTable: true,
                    columnOptions: new ColumnOptions(),
                    useMsi: true)
                .CreateLogger());
        }


        [Fact]
        public void TestIfUseMsiTrueAndAzureServiceTokenProviderResourceIsSet_CreatesLogger()
        {
            // arrange
            var loggerConfiguration = new LoggerConfiguration();
            var logger = loggerConfiguration.AuditTo.MSSqlServer(
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    autoCreateSqlTable: true,
                    columnOptions: new ColumnOptions(),
                    useMsi: true,
                    azureServiceTokenProviderResource: "http://a.com")
                .CreateLogger();
            Assert.True(true);
        }

        public void Dispose()
        {
            DatabaseFixture.DropTable();
        }
    }
}
