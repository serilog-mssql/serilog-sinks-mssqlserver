using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Serilog.Sinks.MSSqlServer.Platform.SqlClient;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlInsertStatementWriter : ISqlLogEventWriter
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ISqlCommandFactory _sqlCommandFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;

        private string _sqlCommandText;

        public SqlInsertStatementWriter(
            string tableName,
            string schemaName,
            ISqlConnectionFactory sqlConnectionFactory,
            ISqlCommandFactory sqlCommandFactory,
            ILogEventDataGenerator logEventDataGenerator)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _sqlConnectionFactory =
                sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _sqlCommandFactory = sqlCommandFactory ?? throw new ArgumentNullException(nameof(sqlCommandFactory));
            _logEventDataGenerator =
                logEventDataGenerator ?? throw new ArgumentNullException(nameof(logEventDataGenerator));
        }

        public void WriteEvent(LogEvent logEvent) => WriteEvents(new[] { logEvent }).GetAwaiter().GetResult();

        public async Task WriteEvents(IEnumerable<LogEvent> events)
        {
            using (var sqlConnection = _sqlConnectionFactory.Create())
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);

                foreach (var logEvent in events)
                {
                    var fields = _logEventDataGenerator.GetColumnsAndValues(logEvent).ToList();
                    using (var sqlCommand = InitializeSqlCommand(sqlConnection, fields))
                    {
                        await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private ISqlCommandWrapper InitializeSqlCommand(
            ISqlConnectionWrapper sqlConnection,
            IEnumerable<KeyValuePair<string, object>> logEventFields)
        {
            InitializeSqlCommandText(logEventFields);

            var sqlCommand = _sqlCommandFactory.CreateCommand(_sqlCommandText, sqlConnection);
            var index = 0;
            foreach (var field in logEventFields)
            {
                sqlCommand.AddParameter(Invariant($"@P{index}"), field.Value);
                index++;
            }

            return sqlCommand;
        }

        private void InitializeSqlCommandText(IEnumerable<KeyValuePair<string, object>> logEventFields)
        {
            if (_sqlCommandText != null)
            {
                return;
            }

            var fieldList = new StringBuilder(Invariant($"INSERT INTO [{_schemaName}].[{_tableName}] ("));
            var parameterList = new StringBuilder(") VALUES (");

            var index = 0;
            foreach (var field in logEventFields)
            {
                if (index != 0)
                {
                    fieldList.Append(',');
                    parameterList.Append(',');
                }

                fieldList.Append(Invariant($"[{field.Key}]"));
                parameterList.Append("@P");
                parameterList.Append(index);

                index++;
            }

            parameterList.Append(')');
            fieldList.Append(parameterList);

            _sqlCommandText = fieldList.ToString();
        }
    }
}
