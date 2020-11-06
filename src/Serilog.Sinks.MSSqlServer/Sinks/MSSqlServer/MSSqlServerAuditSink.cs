// Copyright 2020 Serilog Contributors 
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Platform;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Writes log events as rows in a table of MSSqlServer database using Audit logic, meaning that each row is synchronously committed
    /// and any errors that occur are propagated to the caller.
    /// </summary>
    public class MSSqlServerAuditSink : ILogEventSink, IDisposable
    {
        private readonly ISqlLogEventWriter _sqlLogEventWriter;

        /// <summary>
        /// Construct a sink posting to the specified database.
        ///
        /// Note: this is the legacy version of the extension method. Please use the new one using MSSqlServerSinkOptions instead.
        /// 
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="tableName">Name of the table to store the data in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        [Obsolete("Use the new interface accepting a MSSqlServerSinkOptions parameter instead. This will be removed in a future release.", error: false)]
        public MSSqlServerAuditSink(
            string connectionString,
            string tableName,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable = false,
            ColumnOptions columnOptions = null,
            string schemaName = MSSqlServerSink.DefaultSchemaName,
            ITextFormatter logEventFormatter = null)
            : this(connectionString, new MSSqlServerSinkOptions(tableName, null, null, autoCreateSqlTable, schemaName),
                  formatProvider, columnOptions, logEventFormatter)
        {
            // Do not add new parameters here. This interface is considered legacy and will be deprecated in the future.
            // For adding new input parameters use the MSSqlServerSinkOptions class and the method overload that accepts MSSqlServerSinkOptions.
        }

        /// <summary>
        /// Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="connectionString">Connection string to access the database.</param>
        /// <param name="sinkOptions">Supplies additional options for the sink</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        /// <param name="logEventFormatter">Supplies custom formatter for the LogEvent column, or null</param>
        public MSSqlServerAuditSink(
            string connectionString,
            MSSqlServerSinkOptions sinkOptions,
            IFormatProvider formatProvider = null,
            ColumnOptions columnOptions = null,
            ITextFormatter logEventFormatter = null)
            : this(sinkOptions, columnOptions,
                  SinkDependenciesFactory.Create(connectionString, sinkOptions, formatProvider, columnOptions, logEventFormatter))
        {
        }

        // Internal constructor with injectable dependencies for better testability
        internal MSSqlServerAuditSink(
            MSSqlServerSinkOptions sinkOptions,
            ColumnOptions columnOptions,
            SinkDependencies sinkDependencies)
        {
            ValidateParameters(sinkOptions, columnOptions);
            CheckSinkDependencies(sinkDependencies);

            _sqlLogEventWriter = sinkDependencies.SqlLogEventWriter;

            CreateTable(sinkOptions, sinkDependencies);
        }

        /// <summary>Emit the provided log event to the sink.</summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent) =>
            _sqlLogEventWriter.WriteEvent(logEvent);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Serilog.Sinks.MSSqlServer.MSSqlServerAuditSink and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // This class needn't to dispose anything. This is just here for sink interface compatibility.
        }

        private static void ValidateParameters(MSSqlServerSinkOptions sinkOptions, ColumnOptions columnOptions)
        {
            if (sinkOptions?.TableName == null)
            {
                throw new InvalidOperationException("Table name must be specified!");
            }

            if (columnOptions.DisableTriggers)
                throw new NotSupportedException($"The {nameof(ColumnOptions.DisableTriggers)} option is not supported for auditing.");
        }

        private static void CheckSinkDependencies(SinkDependencies sinkDependencies)
        {
            if (sinkDependencies == null)
            {
                throw new ArgumentNullException(nameof(sinkDependencies));
            }

            if (sinkDependencies.DataTableCreator == null)
            {
                throw new InvalidOperationException($"DataTableCreator is not initialized!");
            }

            if (sinkDependencies.SqlTableCreator == null)
            {
                throw new InvalidOperationException($"SqlTableCreator is not initialized!");
            }

            if (sinkDependencies.SqlLogEventWriter == null)
            {
                throw new InvalidOperationException($"SqlLogEventWriter is not initialized!");
            }
        }

        private static void CreateTable(MSSqlServerSinkOptions sinkOptions, SinkDependencies sinkDependencies)
        {
            if (sinkOptions.AutoCreateSqlTable)
            {
                using (var eventTable = sinkDependencies.DataTableCreator.CreateDataTable())
                {
                    sinkDependencies.SqlTableCreator.CreateTable(eventTable);
                }
            }
        }
    }
}
