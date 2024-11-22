using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
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

        private ISqlCommandWrapper _sqlCommand;
        private bool _disposedValue;

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
            using (var cn = _sqlConnectionFactory.Create())
            {
                await cn.OpenAsync().ConfigureAwait(false);

                foreach (var logEvent in events)
                {
                    var fields = _logEventDataGenerator.GetColumnsAndValues(logEvent).ToList();
                    InitializeSqlCommand(cn, fields);

                    var index = 0;
                    _sqlCommand.ClearParameters();
                    foreach (var field in fields)
                    {
                        _sqlCommand.AddParameter(Invariant($"@P{index}"), field.Value);
                        index++;
                    }

                    await _sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Serilog.Sinks.MSSqlServer.Platform.SqlInsertStatementWriter and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sqlCommand?.Dispose();
                }

                _disposedValue = true;
            }
        }

        private void InitializeSqlCommand(ISqlConnectionWrapper sqlConnection,
            IEnumerable<KeyValuePair<string, object>> logEventFields)
        {
            // Optimization: generate INSERT statement and SqlCommand only once
            // and reuse it with different values and SqlConnections because
            // the structure does not change.
            if (_sqlCommand == null)
            {
                _sqlCommand = _sqlCommandFactory.CreateCommand(sqlConnection);
                _sqlCommand.CommandType = CommandType.Text;

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

                _sqlCommand.CommandText = fieldList.ToString();
            }

            _sqlCommand.SetConnection(sqlConnection);
        }
    }
}
