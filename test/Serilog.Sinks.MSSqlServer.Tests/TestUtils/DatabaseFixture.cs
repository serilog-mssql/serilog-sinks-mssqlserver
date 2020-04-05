using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Dapper;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    public sealed class DatabaseFixture : IDisposable
    {
        public static string Database => "LogTest";
        public static string LogTableName => "LogEvents";

        private const string MasterConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Master;Integrated Security=True";

        private const string CreateLogEventsDatabase = @"
EXEC ('CREATE DATABASE [{0}] ON PRIMARY 
	(NAME = [{0}], 
	FILENAME =''{1}'', 
	SIZE = 25MB, 
	MAXSIZE = 50MB, 
	FILEGROWTH = 5MB )')";

        private static readonly string DatabaseFileNameQuery = $@"SELECT CONVERT(VARCHAR(255), SERVERPROPERTY('instancedefaultdatapath')) + '{Database}.mdf' AS Name";
        private static readonly string DropLogEventsDatabase = $@"
ALTER DATABASE [{Database}]
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE
DROP DATABASE [{Database}]
";

        public static string LogEventsConnectionString => $@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={Database};Integrated Security=True";

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
                conn.Execute($"IF OBJECT_ID('{actualTableName}', 'U') IS NOT NULL DROP TABLE {actualTableName};");
            }
        }

        private static void DeleteDatabase()
        {
            using (var conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();
                var databases = conn.Query("select name from sys.databases");

                if (databases.Any(d => d.name == Database)) conn.Execute(DropLogEventsDatabase);
            }
        }

        private static void CreateDatabase()
        {
            DeleteDatabase();

            using (var conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();
                // ReSharper disable once PossibleNullReferenceException
                var filename = conn.Query<FileName>(DatabaseFileNameQuery).FirstOrDefault().Name;
                var createDatabase = string.Format(CultureInfo.InvariantCulture, CreateLogEventsDatabase, Database, filename);

                conn.Execute(createDatabase);
            }
        }
    }
}
