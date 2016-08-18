using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public static string Database => "LogTest";
        public static string LogTableName => "LogEvents";

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

        public static string MasterConnectionString => @"Data Source=(LocalDb)\v11.0;Initial Catalog=Master;Integrated Security=True";
        public static string LogEventsConnectionString => $@"Data Source=(LocalDb)\v11.0;Initial Catalog={Database};Integrated Security=True";

        public class FileName
        {
            public string Name { get; set; }
        }

        public DatabaseFixture()
        {
            CreateDatabase();
        }
        
        public void Dispose()
        {
            DeleteDatabase();
        }

        private static void DeleteDatabase()
        {
            using (var conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();
                var databases = conn.Query("select name from sys.databases");

                //if (databases.Any(d => d.name == Database)) conn.Execute(DropLogEventsDatabase);
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
                var createDatabase = string.Format(CreateLogEventsDatabase, Database, filename);

                conn.Execute(createDatabase);
            }
        }
    }
}
