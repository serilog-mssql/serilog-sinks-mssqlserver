using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlLogEventsPruner : ISqlLogEventsPruner
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ColumnOptions _columnOptions;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private TimeSpan? _retentionPeriod;
        private TimeSpan? _pruningInterval;
        private long lastCleaningTicks;

        public SqlLogEventsPruner(
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            ISqlConnectionFactory sqlConnectionFactory,
            MSSqlServerSinkOptions MSSqlServerSinkOptions)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _retentionPeriod = MSSqlServerSinkOptions?.RetentionPeriod;
            _pruningInterval = MSSqlServerSinkOptions?.PruningInterval;
            lastCleaningTicks = DateTime.Now.Ticks;
        }

        public async Task PruneLogEventsToDateTime()
        {
            if (_retentionPeriod == null)
            {
                //The default behaviour(without setting the Retention Period in sink options)
                return;
            }
            //Preventing other threads from unnecessary pruning 
            var lastCleaning_ticks = Interlocked.Exchange(ref lastCleaningTicks, DateTime.Now.Ticks);

            try
            {
                if (DateTime.Now.Subtract(new DateTime(lastCleaning_ticks)) < _pruningInterval)
                {
                    Interlocked.Exchange(ref lastCleaningTicks, lastCleaning_ticks);
                    return;
                }
                using (var connection = _sqlConnectionFactory.Create())
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        var deleteQuery = $"DELETE FROM [{_schemaName}].[{_tableName}] WHERE [{_columnOptions.TimeStamp.ColumnName}]<DATEADD(second,-@Seconds,GETDATE())";
                        command.CommandText = deleteQuery;
                        command.AddParameter("@Seconds", ((TimeSpan)_retentionPeriod).TotalSeconds);
                        command.ExecuteNonQuery();
                    }
                }
                Interlocked.Exchange(ref lastCleaningTicks, DateTime.Now.Ticks);
            }
            catch (Exception)
            {
                Interlocked.Exchange(ref lastCleaningTicks, lastCleaning_ticks);
                throw;
            }
        }

    }
}
