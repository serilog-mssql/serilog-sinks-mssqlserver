﻿using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    [CollectionDefinition("DatabaseTests", DisableParallelization = true)]
    public class PatientSecureFixture : ICollectionFixture<DatabaseFixture> { }
}
