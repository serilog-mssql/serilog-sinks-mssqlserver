using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlInsertStatementWriter : ISqlBulkBatchWriter, ISqlLogEventWriter
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;

        public SqlInsertStatementWriter(
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

        public Task WriteBatch(IEnumerable<LogEvent> events, DataTable dataTable) => WriteBatch(events);

        public void WriteEvent(LogEvent logEvent) => WriteBatch(new[] { logEvent }).GetAwaiter().GetResult();

        public async Task WriteBatch(IEnumerable<LogEvent> events)
        {
            try
            {
                using (var cn = _sqlConnectionFactory.Create())
                {
                    await cn.OpenAsync().ConfigureAwait(false);

                    foreach (var logEvent in events)
                    {
                        using (var command = cn.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;

                            var fieldList = new StringBuilder(Invariant($"INSERT INTO [{_schemaName}].[{_tableName}] ("));
                            var parameterList = new StringBuilder(") VALUES (");

                            var index = 0;
                            foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
                            {
                                if (index != 0)
                                {
                                    fieldList.Append(',');
                                    parameterList.Append(',');
                                }

                                fieldList.Append(Invariant($"[{field.Key}]"));
                                parameterList.Append("@P");
                                parameterList.Append(index);

                                command.AddParameter(Invariant($"@P{index}"), field.Value);

                                index++;
                            }

                            parameterList.Append(')');
                            fieldList.Append(parameterList);

                            command.CommandText = fieldList.ToString();

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write log event to the database due to following error: {0}", ex);
                throw;
            }
        }
    }
}
