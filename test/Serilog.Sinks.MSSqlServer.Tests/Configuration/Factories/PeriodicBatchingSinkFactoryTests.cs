using Moq;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Sinks.PeriodicBatching;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Factories
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class PeriodicBatchingSinkFactoryTests
    {
        [Fact]
        public void PeriodicBatchingSinkFactoryCreateReturnsInstance()
        {
            // Arrange
            var sinkMock = new Mock<IBatchedLogEventSink>();
            var sut = new PeriodicBatchingSinkFactory();

            // Act
            var result = sut.Create(sinkMock.Object, new SinkOptions());

            // Assert
            Assert.IsType<PeriodicBatchingSink>(result);
        }
    }
}
