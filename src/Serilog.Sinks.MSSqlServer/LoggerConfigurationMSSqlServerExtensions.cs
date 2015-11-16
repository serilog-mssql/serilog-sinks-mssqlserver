using System;
using System.Data;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Configuration;

// Copyright 2014 Serilog Contributors
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

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.MSSqlServer() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationMSSqlServerExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="storeProperties">Indicates if the additional properties need to be stored as well.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="storeTimestampInUtc">Store Timestamp In UTC</param>
        /// <param name="additionalDataColumns">Additional columns for data storage.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MSSqlServer(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString =null, string tableName=null, bool storeProperties = true,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = MSSqlServerSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            bool storeTimestampInUtc = false,
            DataColumn[] additionalDataColumns = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            var defaultedPeriod = period ?? MSSqlServerSink.DefaultPeriod;

            MSSqlServerConfigurationSection serviceConfigSection =
               ConfigurationManager.GetSection("MSSqlServerSettingsSection") as MSSqlServerConfigurationSection;
            // If we have additional columns from config, load them as well
            if (serviceConfigSection.Columns.Count > 0)
            {
                additionalDataColumns = GenerateDataColumnsFromConfig(serviceConfigSection, additionalDataColumns);
            }

            return loggerConfiguration.Sink(
                new MSSqlServerSink(connectionString, tableName, storeProperties, batchPostingLimit, defaultedPeriod, formatProvider, storeTimestampInUtc, additionalDataColumns),
                restrictedToMinimumLevel);
        }

        /// <summary>
        /// Generate an array of DataColumns using the supplied MSSqlServerConfigurationSection,
        ///     which is an array of keypairs defining the SQL column name and SQL data type
        /// Entries are appended to an incoming list of DataColumns in addiitonalColumns
        /// </summary>
        /// <param name="serviceConfigSection">A previously loaded configuration section</param>
        /// <param name="additionalColumns">Existing array of columns to append our config columns to</param>
        /// <returns></returns>
        private static DataColumn[] GenerateDataColumnsFromConfig(MSSqlServerConfigurationSection serviceConfigSection, DataColumn[] additionalColumns)
        {
            int i = 0;
            DataColumn[] returnColumns;
            if (additionalColumns == null)
            {
                returnColumns = new DataColumn[serviceConfigSection.Columns.Count];
            }
            else
            {
                returnColumns = additionalColumns;
                Array.Resize<DataColumn>( ref returnColumns, serviceConfigSection.Columns.Count + additionalColumns.Length);
                i = additionalColumns.Length;
            }
            //int arraySize = additionalColumns == null ? 0 : additionalColumns.Length;
            //DataColumn[] returnColumns = new DataColumn[serviceConfigSection.Columns.Count];
            //int i = 0;

            foreach (ColumnConfig c in serviceConfigSection.Columns)
            {
                // Set the type based on the defined SQL type from config
                DataColumn column = new DataColumn(c.ColumnName);
                Type dataType = null;

                switch (c.DataType)
                {
                    case "bigint":
                        dataType = Type.GetType("System.Int64");
                        break;
                    case "bit":
                        dataType = Type.GetType("System.Boolean");
                        break;
                    case "char":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "varchar":
                        dataType = Type.GetType("System.String");
                        break;
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "smalldatetime":
                        dataType = Type.GetType("System.DateTime");
                        break;
                    case "decimal":
                    case "money":
                    case "numeric":
                    case "smallmoney":
                        dataType = Type.GetType("System.Decimal");
                        break;
                    case "float":
                        dataType = Type.GetType("System.Double");
                        break;
                    case "int":
                        dataType = Type.GetType("System.Int32");
                        break;
                    case "real":
                        dataType = Type.GetType("System.Single");
                        break;
                    case "smallint":
                        dataType = Type.GetType("System.Int16");
                        break;
                    case "time":
                        dataType = Type.GetType("System.TimeSpan");
                        break;
                    case "uniqueidentifier":
                        dataType = Type.GetType("System.Guid");
                        break;
                }
                returnColumns[i++] = column;
            }

            return returnColumns;
        }
    }
}
