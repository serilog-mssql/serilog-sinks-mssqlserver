using System.Diagnostics;
using System.Globalization;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.Misc
{
    [Trait(TestCategory.TraitName, TestCategory.Integration)]
    public class OpenTelemetryColumnsTests : DatabaseTestsBase
    {
        public OpenTelemetryColumnsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void OpenTelemetryActivityTraceIdAndSpanIdAreStoredInColumns()
        {
            // Arrange
            var expectedTraceId = string.Empty;
            var expectedSpanId = string.Empty;
            var columnOptions = new MSSqlServer.ColumnOptions();
            columnOptions.Store.Add(StandardColumn.TraceId);
            columnOptions.Store.Add(StandardColumn.SpanId);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = DatabaseFixture.LogTableName,
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions,
                    formatProvider: CultureInfo.InvariantCulture
                )
                .CreateLogger();

            // Act
            using (var testActivity = new Activity("OpenTelemetryColumnsTests"))
            {
                testActivity.SetIdFormat(ActivityIdFormat.W3C);
                testActivity.Start();
                expectedTraceId = testActivity.TraceId.ToString();
                expectedSpanId = testActivity.SpanId.ToString();


                Log.Logger.Information("Logging message");
                Log.CloseAndFlush();
            }

            // Assert
            VerifyStringColumnWritten("TraceId", expectedTraceId);
            VerifyStringColumnWritten("SpanId", expectedSpanId);
        }
    }
}
