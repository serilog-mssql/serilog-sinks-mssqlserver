using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    [CollectionDefinition("LogTest", DisableParallelization = true)]
    public class PatientSecureFixtureCollection : ICollectionFixture<DatabaseFixture> { }
}
