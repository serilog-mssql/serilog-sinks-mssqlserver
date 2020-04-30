using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;

namespace Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Platform
{
    internal class SqlLogEventWriter : ISqlLogEventWriter
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;

        public SqlLogEventWriter(
            string tableName,
            string schemaName,
            ISqlConnectionFactory sqlConnectionFactory,
            ILogEventDataGenerator logEventDataGenerator)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _logEventDataGenerator = logEventDataGenerator ?? throw new ArgumentNullException(nameof(logEventDataGenerator));
        }

        public void WriteEvent(LogEvent logEvent)
        {
            try
            {
                using (var connection = _sqlConnectionFactory.Create())
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;

                        var fieldList = new StringBuilder($"INSERT INTO [{_schemaName}].[{_tableName}] (");
                        var parameterList = new StringBuilder(") VALUES (");

                        var index = 0;
                        foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
                        {
                            if (index != 0)
                            {
                                fieldList.Append(',');
                                parameterList.Append(',');
                            }

                            fieldList.Append(field.Key);
                            parameterList.Append("@P");
                            parameterList.Append(index);

                            var parameter = new SqlParameter($"@P{index}", field.Value ?? DBNull.Value);

                            // The default is SqlDbType.DateTime, which will truncate the DateTime value if the actual
                            // type in the database table is datetime2. So we explicitly set it to DateTime2, which will
                            // work both if the field in the table is datetime and datetime2, which is also consistent with 
                            // the behavior of the non-audit sink.
                            if (field.Value is DateTime)
                                parameter.SqlDbType = SqlDbType.DateTime2;

                            command.Parameters.Add(parameter);

                            index++;
                        }

                        parameterList.Append(')');
                        fieldList.Append(parameterList.ToString());

                        command.CommandText = fieldList.ToString();

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write log event to the database due to following error: {1}", ex.Message);
                throw;
            }
        }
    }
}
