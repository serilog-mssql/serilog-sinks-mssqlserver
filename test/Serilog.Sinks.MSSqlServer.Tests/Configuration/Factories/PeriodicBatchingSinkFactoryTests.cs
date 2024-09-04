using Moq;
using Serilog.Sinks.MSSqlServer.Configuration.Factories;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Core;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Factories
{
    // BatchingSink is not public
    // temporarily removing this test

    //[Trait(TestCategory.TraitName, TestCategory.Unit)]
    //public class PeriodicBatchingSinkFactoryTests
    //{
    //    [Fact]
    //    public void PeriodicBatchingSinkFactoryCreateReturnsInstance()
    //    {
    //        // Arrange
    //        var sinkMock = new Mock<IBatchedLogEventSink>();
    //        var sut = new PeriodicBatchingSinkFactory();

    //        // Act
    //        var result = sut.Create(sinkMock.Object, new MSSqlServerSinkOptions());

    //        // Assert
    //        Assert.IsType<BatchingSink>(result);
    //    }
    //}
}
