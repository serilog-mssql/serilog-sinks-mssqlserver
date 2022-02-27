using Serilog.Configuration;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.System.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SystemConfigurationSinkOptionsProviderTests
    {
        [Fact]
        public void ConfigureSinkOptionsReadsBatchSettings()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.EagerlyEmitFirstEvent.Value = "true";
            var sinkOptions = new MSSqlServerSinkOptions { EagerlyEmitFirstEvent = false };
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.True(sinkOptions.EagerlyEmitFirstEvent);
        }

        [Fact]
        public void ConfigureSinkOptionsReadsPreventEnlistInTransaction()
        {
            // Arrange
            var configSection = new MSSqlServerConfigurationSection();
            configSection.PreventEnlistInTransaction.Value = "false";
            var sinkOptions = new MSSqlServerSinkOptions { PreventEnlistInTransaction = true };
            var sut = new SystemConfigurationSinkOptionsProvider();

            // Act
            sut.ConfigureSinkOptions(configSection, sinkOptions);

            // Assert
            Assert.False(sinkOptions.PreventEnlistInTransaction);
        }
    }
}
