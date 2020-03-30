using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Collection("LogTest")]
    public class SqlTypesTests : DatabaseTestsBase
    {
        // Since the point of these tests are to validate we can write to
        // specific underlying SQL Server column data types, we use audit
        // logging so exceptions from logevent writes do not fail silently.

        [Fact]
        public void AuditLogNumericSqlTypes()
        {
            Arrange(new Collection<SqlColumn>
            {
                new SqlColumn("BigInt", SqlDbType.BigInt),
                new SqlColumn("Decimal", SqlDbType.Decimal),
                new SqlColumn("Float", SqlDbType.Float),
                new SqlColumn("Int", SqlDbType.Int),
                new SqlColumn("Money", SqlDbType.Money),
                new SqlColumn("Real", SqlDbType.Real),
                new SqlColumn("SmallInt", SqlDbType.SmallInt),
                new SqlColumn("SmallMoney", SqlDbType.SmallMoney),
                new SqlColumn("TinyInt", SqlDbType.TinyInt),
            });

            // Some underlying .NET equivalents have a higher MaxValue than the SQL types' defaults.
            decimal maxDecimal = 999999999999999999M;
            decimal maxMoney = 922337203685477.5807M;
            decimal maxSmallMoney = 214748.3647M;

            // success: should not throw

            Log.Information("BigInt {BigInt}", long.MaxValue);
            Log.Information("Decimal {Decimal}", maxDecimal);
            Log.Information("Float {Float}", double.MaxValue);
            Log.Information("Int {Int}", int.MaxValue);
            Log.Information("Money {Money}", maxMoney);
            Log.Information("Real {Real}", float.MaxValue);
            Log.Information("SmallInt {SmallInt}", short.MaxValue);
            Log.Information("SmallMoney {SmallMoney}", maxSmallMoney);
            Log.Information("TinyInt {TinyInt}", byte.MaxValue);
            Log.CloseAndFlush();
        }

        [Fact]
        public void AuditLogDateAndTimeSqlTypes()
        {
            Arrange(new Collection<SqlColumn>
            {
                new SqlColumn("Date", SqlDbType.Date),
                new SqlColumn("DateTime", SqlDbType.DateTime),
                new SqlColumn("DateTime2", SqlDbType.DateTime2),
                new SqlColumn("DateTimeOffset", SqlDbType.DateTimeOffset),
                new SqlColumn("SmallDateTime", SqlDbType.SmallDateTime),
                new SqlColumn("Time", SqlDbType.Time),
            });

            // .NET DateTime is limited to 999ms, some of the SQL types have higher precision as noted
            DateTime maxDate = new DateTime(9999, 12, 31);
            DateTime maxDateTime = new DateTime(9999,12,31,23,59,59,997);
            DateTime maxDateTime2 = new DateTime(9999, 12, 31, 23, 59, 59, 999); // SQL max 9999999ms
            DateTimeOffset maxDateTimeOffset = new DateTimeOffset(9999, 12, 31, 23, 59, 59, 999, TimeSpan.FromMinutes(0)); // SQL max 9999999ms
            DateTime maxSmallDateTime = new DateTime(2079, 6, 6, 23, 59, 0); // seconds round up or down, 59 seconds will overflow here
            TimeSpan maxTime = new TimeSpan(0, 23, 59, 59, 999); // SQL max 9999999ms

            // sucess: should not throw

            Log.Information("Date {Date}", maxDate);
            Log.Information("DateTime {DateTime}", maxDateTime);
            Log.Information("DateTime2 {DateTime2}", maxDateTime2);
            Log.Information("DateTimeOffset {datDateTimeOffsete}", maxDateTimeOffset);
            Log.Information("SmallDateTime {SmallDateTime}", maxSmallDateTime);
            Log.Information("Time {Time}", maxTime);
            Log.CloseAndFlush();
        }

        [Fact]
        public void AuditLogCharacterDataSqlTypes()
        {
            Arrange(new Collection<SqlColumn>
            {
                new SqlColumn("Char", SqlDbType.Char, dataLength: 20),
                new SqlColumn("NChar", SqlDbType.NChar, dataLength: 20),
                new SqlColumn("NVarChar", SqlDbType.NVarChar, dataLength: 20),
                new SqlColumn("VarChar", SqlDbType.VarChar, dataLength: 20),
            });

            string twentyChars = new string('x', 20);
            string thirtyChars = new string('x', 30);

            // sucess: should not throw

            Log.Information("Char {Char}", twentyChars);
            Log.Information("NChar {NChar}", twentyChars);
            Log.Information("NVarChar {NVarChar}", twentyChars);
            Log.Information("VarChar {VarChar}", twentyChars);

            // should throw truncation exception

            Assert.Throws<AggregateException>(() => Log.Information("Char {Char}", thirtyChars));

            Log.CloseAndFlush();
        }

        [Fact]
        public void AuditLogMiscellaneousSqlTypes()
        {
            Arrange(new Collection<SqlColumn>
            {
                new SqlColumn("UniqueIdentifier", SqlDbType.UniqueIdentifier),
                new SqlColumn("Xml", SqlDbType.Xml)
            });

            // SQL Server will convert strings internally automatically
            string xmlAsStringData = "<?xml version=\"1.0\"?><test><node attrib = \"value1\">value2</node></test>";

            // sucess: should not throw

            Log.Information("UniqueIdentifier {UniqueIdentifier}", Guid.NewGuid());
            Log.Information("Xml {Xml}", xmlAsStringData);
            Log.CloseAndFlush();
        }

        private void Arrange(ICollection<SqlColumn> customColumns)
        {
            var columnOptions = new ColumnOptions();
            columnOptions.AdditionalColumns = customColumns;

            Log.Logger = new LoggerConfiguration()
                .AuditTo.MSSqlServer
                (
                    connectionString: DatabaseFixture.LogEventsConnectionString,
                    tableName: DatabaseFixture.LogTableName,
                    columnOptions: columnOptions,
                    autoCreateSqlTable: true
                )
                .CreateLogger();
        }
    }
}
