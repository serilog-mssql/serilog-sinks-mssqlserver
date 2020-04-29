using System.Data;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class DataTableCreator : IDataTableCreator
    {
        public DataTable CreateDataTable(string tableName, ColumnOptions columnOptions)
        {
            var eventsTable = new DataTable(tableName);

            foreach (var standardColumn in columnOptions.Store)
            {
                var standardOpts = columnOptions.GetStandardColumnOptions(standardColumn);
                var dataColumn = standardOpts.AsDataColumn();
                eventsTable.Columns.Add(dataColumn);
                if (standardOpts == columnOptions.PrimaryKey)
                    eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
            }

            if (columnOptions.AdditionalColumns != null)
            {
                foreach (var addCol in columnOptions.AdditionalColumns)
                {
                    var dataColumn = addCol.AsDataColumn();
                    eventsTable.Columns.Add(dataColumn);
                    if (addCol == columnOptions.PrimaryKey)
                        eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
                }
            }

            return eventsTable;
        }
    }
}
