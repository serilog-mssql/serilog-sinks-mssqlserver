using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Factories
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MSSqlServerSinkFactoryTests
    {
        [Fact]
        public void MSSqlServerSinkFactoryCreateReturnsInstance()
        {
            // Arrange
            var sinkOptions = new SinkOptions { TableName = "TestTableName" };
            var sut = new MSSqlServerSinkFactory();

            // Act
            var result = sut.Create("TestConnectionString", sinkOptions, null, new MSSqlServer.ColumnOptions(), null);

            // Assert
            Assert.IsType<MSSqlServerSink>(result);
        }
    }
}
