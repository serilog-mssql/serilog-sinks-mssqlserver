using Microsoft.Data.SqlClient;

namespace Serilog.Sinks.MSSqlServer.Platform.SqlClient
{
    internal class SqlConnectionStringBuilderWrapper : ISqlConnectionStringBuilderWrapper
    {
        private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;

        public SqlConnectionStringBuilderWrapper()
        {
            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
        }

        public string ConnectionString
        {
            get => _sqlConnectionStringBuilder.ConnectionString;
            set => _sqlConnectionStringBuilder.ConnectionString = value;
        }

        public bool Enlist
        {
            set => _sqlConnectionStringBuilder.Enlist = value;
        }
    }
}
