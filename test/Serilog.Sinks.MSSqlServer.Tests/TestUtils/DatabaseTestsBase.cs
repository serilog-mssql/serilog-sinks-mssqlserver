using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Dapper;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    [Collection("DatabaseTests")]
    public abstract class DatabaseTestsBase : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private bool _disposedValue;

        protected DatabaseTestsBase(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));

            Serilog.Debugging.SelfLog.Enable(_output.WriteLine);
        }

        protected static void VerifyDatabaseColumnsWereCreated(IEnumerable<string> columnNames)
        {
            if (columnNames == null)
            {
                return;
            }

            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var column in columnNames)
                {
                    infoSchema.Should().Contain(columns => columns.ColumnName == column);
                }

                infoSchema.Should().Contain(columns => columns.ColumnName == "Id");
            }
        }

        protected static void VerifyDatabaseColumnsWereCreated(IEnumerable<SqlColumn> columnDefinitions)
        {
            if (columnDefinitions == null)
            {
                return;
            }

            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName, UPPER(DATA_TYPE) as DataType, CHARACTER_MAXIMUM_LENGTH as DataLength, IS_NULLABLE as AllowNull
                    FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                foreach (var definition in columnDefinitions)
                {
                    var column = infoSchema.SingleOrDefault(c => c.ColumnName == definition.ColumnName);
                    Assert.NotNull(column);
                    var definitionDataType = definition.DataType.ToString().ToUpperInvariant();
                    Assert.Equal(definitionDataType, column.DataType);
                    if (definitionDataType == "NVARCHAR" || definitionDataType == "VARCHAR")
                    {
                        Assert.Equal(definition.DataLength.ToString(CultureInfo.InvariantCulture), column.DataLength);
                    }
                    if (definition.AllowNull)
                    {
                        Assert.Equal("YES", column.AllowNull);
                    }
                    else
                    {
                        Assert.Equal("NO", column.AllowNull);
                    }
                }
            }
        }

        protected static void VerifyIdColumnWasCreatedAndHasIdentity(string idColumnName = "Id")
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<InfoSchema>($@"SELECT COLUMN_NAME AS ColumnName FROM {DatabaseFixture.Database}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DatabaseFixture.LogTableName}'");
                var infoSchema = logEvents as InfoSchema[] ?? logEvents.ToArray();

                infoSchema.Should().Contain(columns => columns.ColumnName == idColumnName);

                var isIdentity = conn.Query<IdentityQuery>($"SELECT COLUMNPROPERTY(object_id('{DatabaseFixture.LogTableName}'), '{idColumnName}', 'IsIdentity') AS IsIdentity");

                isIdentity.Should().Contain(i => i.IsIdentity == 1);
            }
        }

        protected static void VerifyLogMessageWasWritten(string expectedMessage)
        {
            //using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            //{
            //    var logEvents = conn.Query<DefaultStandardLogColumns>($"SELECT * FROM {DatabaseFixture.LogTableName}");

            //    logEvents.Should().Contain(e => e.Message.Contains(message));
            //}

            VerifyStringColumnWritten("Message", expectedMessage);
        }

        protected static void VerifyStringColumnWritten(string columnName, string expectedValue)
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<string>($"SELECT {columnName} FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(c => c == expectedValue);
            }
        }

        protected static void VerifyIntegerColumnWritten(string columnName, int expectedValue)
        {
            using (var conn = new SqlConnection(DatabaseFixture.LogEventsConnectionString))
            {
                var logEvents = conn.Query<int>($"SELECT {columnName} FROM {DatabaseFixture.LogTableName}");

                logEvents.Should().Contain(c => c == expectedValue);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                DatabaseFixture.DropTable();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
