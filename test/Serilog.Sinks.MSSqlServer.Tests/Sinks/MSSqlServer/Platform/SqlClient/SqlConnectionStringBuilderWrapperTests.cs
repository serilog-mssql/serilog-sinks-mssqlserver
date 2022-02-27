#if NET452
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using Xunit;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using System;

namespace Serilog.Sinks.MSSqlServer.Tests.Platform.SqlClient
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class SqlConnectionStringBuilderWrapperTests
    {
        [Fact]
        public void CreatesSqlConnectionString()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper();

            // Act
            sut.ConnectionString = DatabaseFixture.LogEventsConnectionString;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString, sut.ConnectionString);
        }

        [Fact]
        public void DoesNotAddEnlistIfEnlistPropertyIsNotSet()
        {
            // Arrange + Act
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = DatabaseFixture.LogEventsConnectionString
            };

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString, sut.ConnectionString);
        }

        [Fact]
        public void DoesNotChangeEnlistFalseIfEnlistPropertyIsNotSet()
        {
            // Arrange + Act
            var connectionStringEnlistFalse = DatabaseFixture.LogEventsConnectionString + ";Enlist=True";
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = connectionStringEnlistFalse
            };

            // Assert
            Assert.Equal(connectionStringEnlistFalse, sut.ConnectionString);
        }

        [Fact]
        public void DoesNotChangeEnlistTrueIfEnlistPropertyIsNotSet()
        {
            // Arrange + Act
            var connectionStringEnlistFalse = DatabaseFixture.LogEventsConnectionString + ";Enlist=True";
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = connectionStringEnlistFalse
            };

            // Assert
            Assert.Equal(connectionStringEnlistFalse, sut.ConnectionString);
        }

        [Fact]
        public void ChangeEnlistFalseToTrueIfEnlistPropertyIsSetToTrue()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = DatabaseFixture.LogEventsConnectionString + ";Enlist=False"
            };

            // Act
            sut.Enlist = true;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString + ";Enlist=True", sut.ConnectionString);
        }

        [Fact]
        public void ChangeEnlistTrueToFalseIfEnlistPropertyIsSetToFalse()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = DatabaseFixture.LogEventsConnectionString + ";Enlist=True"
            };

            // Act
            sut.Enlist = false;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString + ";Enlist=False", sut.ConnectionString);
        }

        [Fact]
        public void AddsEnlistFalseIfEnlistPropertySetToFalse()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = DatabaseFixture.LogEventsConnectionString,
            };

            // Act
            sut.Enlist = false;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString + ";Enlist=False", sut.ConnectionString);
        }

        [Fact]
        public void AddsEnlistTrueIfEnlistPropertySetToTrue()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = DatabaseFixture.LogEventsConnectionString,
            };

            // Act
            sut.Enlist = true;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString + ";Enlist=True", sut.ConnectionString);
        }

        [Fact]
        public void DoesNotDuplicateEnlistIfEnlistFalseIsPresentAndEnlistPropertySetToFalse()
        {
            // Arrange
            var sut = new SqlConnectionStringBuilderWrapper
            {
                ConnectionString = "Enlist = false ; " + DatabaseFixture.LogEventsConnectionString
            };

            // Act
            sut.Enlist = false;

            // Assert
            Assert.Equal(DatabaseFixture.LogEventsConnectionString + ";Enlist=False", sut.ConnectionString);
        }

    }
}
