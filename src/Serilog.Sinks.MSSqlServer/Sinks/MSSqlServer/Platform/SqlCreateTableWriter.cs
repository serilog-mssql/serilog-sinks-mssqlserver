using System;
using System.Data;
using System.Globalization;
using System.Text;
using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlCreateTableWriter : ISqlCreateTableWriter
    {
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly ColumnOptions _columnOptions;
        private readonly IDataTableCreator _dataTableCreator;

        public SqlCreateTableWriter(string schemaName, string tableName, ColumnOptions columnOptions, IDataTableCreator dataTableCreator)
        {
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _columnOptions = columnOptions ?? throw new ArgumentNullException(nameof(columnOptions));
            _dataTableCreator = dataTableCreator ?? throw new ArgumentNullException(nameof(dataTableCreator));
        }

        public string TableName => _tableName;

        public string GetSql()
        {
            var sql = new StringBuilder();
            var ix = new StringBuilder();
            var indexCount = 1;

            // start schema check and DDL (wrap in EXEC to make a separate batch)
            sql.AppendLine(Invariant($"IF(NOT EXISTS(SELECT * FROM sys.schemas WHERE name = '{_schemaName}'))"));
            sql.AppendLine("BEGIN");
            sql.AppendLine(Invariant($"EXEC('CREATE SCHEMA [{_schemaName}] AUTHORIZATION [dbo]')"));
            sql.AppendLine("END");

            // start table-creatin batch and DDL
            sql.AppendLine(Invariant($"IF NOT EXISTS (SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{_schemaName}' AND t.name = '{_tableName}')"));
            sql.AppendLine("BEGIN");
            sql.AppendLine(Invariant($"CREATE TABLE [{_schemaName}].[{_tableName}] ( "));

            using (var dataTable = _dataTableCreator.CreateDataTable())
            {
                // build column list
                var i = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    var common = (SqlColumn)column.ExtendedProperties["SqlColumn"];

                    sql.Append(GetColumnDdl(common));
                    if (dataTable.Columns.Count > i++) sql.Append(',');
                    sql.AppendLine();

                    // collect non-PK indexes for separate output after the table DDL
                    if (common != null && common.NonClusteredIndex && common != _columnOptions.PrimaryKey)
                        ix.AppendLine(Invariant($"CREATE NONCLUSTERED INDEX [IX{indexCount++}_{_tableName}] ON [{_schemaName}].[{_tableName}] ([{common.ColumnName}]);"));
                }
            }

            // primary key constraint at the end of the table DDL
            if (_columnOptions.PrimaryKey != null)
            {
                var clustering = (_columnOptions.PrimaryKey.NonClusteredIndex ? "NON" : string.Empty);
                sql.AppendLine(Invariant($" CONSTRAINT [PK_{_tableName}] PRIMARY KEY {clustering}CLUSTERED ([{_columnOptions.PrimaryKey.ColumnName}])"));
            }

            // end of CREATE TABLE
            sql.AppendLine(");");

            // CCI is output separately after table DDL
            if (_columnOptions.ClusteredColumnstoreIndex)
                sql.AppendLine(Invariant($"CREATE CLUSTERED COLUMNSTORE INDEX [CCI_{_tableName}] ON [{_schemaName}].[{_tableName}]"));

            // output any extra non-clustered indexes
            sql.Append(ix);

            // end of batch
            sql.AppendLine("END");

            return sql.ToString();
        }

        // Examples of possible output:
        // [Id] BIGINT IDENTITY(1,1) NOT NULL
        // [Message] VARCHAR(1024) NULL
        private static string GetColumnDdl(SqlColumn column)
        {
            var sb = new StringBuilder();

            sb.Append(Invariant($"[{column.ColumnName}] "));

            sb.Append(column.DataType.ToString().ToUpperInvariant());

            if (SqlDataTypes.DataLengthRequired.Contains(column.DataType))
                sb.Append('(').Append(column.DataLength == -1 ? "MAX" : column.DataLength.ToString(CultureInfo.InvariantCulture)).Append(')');

            if (column.StandardColumnIdentifier == StandardColumn.Id)
                sb.Append(" IDENTITY(1,1)");

            sb.Append(column.AllowNull ? " NULL" : " NOT NULL");

            return sb.ToString();
        }
    }
}
