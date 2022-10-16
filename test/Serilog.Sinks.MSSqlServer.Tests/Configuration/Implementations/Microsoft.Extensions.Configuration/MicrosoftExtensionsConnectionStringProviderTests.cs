using Microsoft.Extensions.Configuration;
using Moq;
using Serilog.Sinks.MSSqlServer.Configuration;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Configuration.Implementations.Microsoft.Extensions.Configuration
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MicrosoftExtensionsConnectionStringProviderTests
    {
        [Fact]
        public void GetConnectionStringCalledWithConnectionStringReturnsSameValue()
        {
            // Arrange
            const string connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;Encrypt=False;";
            var configurationMock = new Mock<IConfiguration>();
            var sut = new MicrosoftExtensionsConnectionStringProvider();

            // Act
            var result = sut.GetConnectionString(connectionString, configurationMock.Object);

            // Assert
            Assert.Equal(connectionString, result);
        }

        [Fact]
        public void GetConnectionStringCalledWithNameItGetsConnectionStringFromConfig()
        {
            // Arrange
            const string connectionStringName = "LogDatabase";
            const string connectionString = "Server=localhost;Database=LogTest;Integrated Security=SSPI;Encrypt=False;";
            var configurationMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configSectionMock.Object);
            configSectionMock.Setup(c => c[It.IsAny<string>()]).Returns(connectionString);
            var sut = new MicrosoftExtensionsConnectionStringProvider();

            // Act
            var result = sut.GetConnectionString(connectionStringName, configurationMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetSection("ConnectionStrings"), Times.Once);
            configSectionMock.Verify(c => c[connectionStringName], Times.Once);
            Assert.Equal(connectionString, result);
        }
    }
}
