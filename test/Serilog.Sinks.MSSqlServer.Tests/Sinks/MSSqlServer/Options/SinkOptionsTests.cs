using System;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Options
{
    public class SinkOptionsTests
    {
        [Fact]
        public void InitializesDefaultedPropertiesWithDefaultsWhenCalledWithoutParameters()
        {
            // Act
            var sut = new SinkOptions();

            // Assert
            Assert.Equal(MSSqlServerSink.DefaultSchemaName, sut.SchemaName);
            Assert.Equal(MSSqlServerSink.DefaultBatchPostingLimit, sut.BatchPostingLimit);
            Assert.Equal(MSSqlServerSink.DefaultPeriod, sut.BatchPeriod);
        }

        [Fact]
        public void InitializesDefaultedPropertiesWithDefaultsWhenCalledWithParameters()
        {
            // Act
            var sut = new SinkOptions("TestTableName", null, null, true, null);

            // Assert
            Assert.Equal(MSSqlServerSink.DefaultSchemaName, sut.SchemaName);
            Assert.Equal(MSSqlServerSink.DefaultBatchPostingLimit, sut.BatchPostingLimit);
            Assert.Equal(MSSqlServerSink.DefaultPeriod, sut.BatchPeriod);
        }

        [Fact]
        public void InitializesPropertiesWithParameterValues()
        {
            // Arrange
            const string tableName = "TestTableName";
            const int batchPostingLimit = 23;
            const string schemaName = "TestSchemaName";
            var batchPeriod = new TimeSpan(0, 3, 23);

            // Act
            var sut = new SinkOptions(tableName, batchPostingLimit, batchPeriod, true, schemaName);

            // Assert
            Assert.Equal(tableName, sut.TableName);
            Assert.Equal(batchPostingLimit, sut.BatchPostingLimit);
            Assert.Equal(batchPeriod, sut.BatchPeriod);
            Assert.True(sut.AutoCreateSqlTable);
            Assert.Equal(schemaName, sut.SchemaName);
        }
    }
}
