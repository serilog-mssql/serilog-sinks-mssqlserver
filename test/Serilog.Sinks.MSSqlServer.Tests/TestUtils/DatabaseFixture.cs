using System;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    public sealed class DatabaseFixture : IDisposable
    {

        private const string _masterConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Master;Integrated Security=True;Connect Timeout=120";
        private const string _createLogEventsDatabase = @"
EXEC ('CREATE DATABASE [{0}] ON PRIMARY 
	(NAME = [{0}], 
	FILENAME =''{1}'', 
	SIZE = 25MB, 
	MAXSIZE = 50MB, 
	FILEGROWTH = 5MB )')";

        private static readonly string _databaseFileNameQuery = Invariant($@"SELECT CONVERT(VARCHAR(255), SERVERPROPERTY('instancedefaultdatapath')) + '{Database}.mdf' AS Name");
        private static readonly string _dropLogEventsDatabase = Invariant($@"
ALTER DATABASE [{Database}]
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE
DROP DATABASE [{Database}]
");

        public static string Database => "LogTest";
        public static string LogTableName => "LogEvents";
        public static string LogEventsConnectionString => Invariant($@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={Database};Integrated Security=True");

        public DatabaseFixture()
        {
            CreateDatabase();
        }

        public void Dispose()
        {
            DeleteDatabase();
        }

        public static void DropTable(string tableName = null)
        {
            using (var conn = new SqlConnection(LogEventsConnectionString))
            {
                var actualTableName = string.IsNullOrEmpty(tableName) ? LogTableName : tableName;
                conn.Execute(Invariant($"IF OBJECT_ID('{actualTableName}', 'U') IS NOT NULL DROP TABLE {actualTableName};"));
            }
        }

        private static void DeleteDatabase()
        {
            using (var conn = new SqlConnection(_masterConnectionString))
            {
                conn.Open();
                var databases = conn.Query("select name from sys.databases");

                if (databases.Any(d => d.name == Database)) conn.Execute(_dropLogEventsDatabase);
            }
        }

        private static void CreateDatabase()
        {
            DeleteDatabase();

            using (var conn = new SqlConnection(_masterConnectionString))
            {
                conn.Open();
                // ReSharper disable once PossibleNullReferenceException
                var filename = conn.Query<FileName>(_databaseFileNameQuery).FirstOrDefault().Name;
                var createDatabase = string.Format(CultureInfo.InvariantCulture, _createLogEventsDatabase, Database, filename);

                conn.Execute(createDatabase);
            }
        }
    }
}
