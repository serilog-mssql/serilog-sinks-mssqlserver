using System;
using System.Collections.ObjectModel;
using Serilog.Debugging;

namespace Serilog.Sinks.MSSqlServer
{
    public partial class ColumnOptions
    {
        private bool _configurationFinalized = false;

        /// <summary>
        /// The logging sink and audit sink constructors call this. Defaults are resolved (like ensuring the
        /// primary key is non-null) and obsolete features are migrated to their replacement features so
        /// dependencies in the sink itself can be safely removed as early as possible.
        /// </summary>
        internal void FinalizeConfigurationForSinkConstructor()
        {
            if (_configurationFinalized)
                return;

#pragma warning disable 618 // deprecated: ColumnOptions.AddtionalDataColumns
            if (AdditionalDataColumns != null)
            {
                SelfLog.WriteLine("Deprecated: The \"AdditionalDataColumns\" collection will be removed in a future release. Please use the \"AdditionalColumns\" collection.");

                if (AdditionalColumns == null)
                    AdditionalColumns = new Collection<SqlColumn>();

                foreach (var dataColumn in AdditionalDataColumns)
                {
                    AdditionalColumns.Add(new SqlColumn(dataColumn));
                }
                AdditionalDataColumns = null;
            }
#pragma warning restore 618

            // the constructor sets Id as the PK, remove it if the Id column was removed
            if (!Store.Contains(StandardColumn.Id) && PrimaryKey == Id)
                PrimaryKey = null;

            if (ClusteredColumnstoreIndex)
            {
                if (PrimaryKey != null)
                {
                    PrimaryKey = null;
                    SelfLog.WriteLine("Warning: Removing primary key, incompatible with clustered columnstore indexing.");
                }

                foreach (var stdcol in Store)
                    ColumnstoreCompatibilityCheck(GetStandardColumnOptions(stdcol));
            }

            if (AdditionalColumns != null)
            {
                foreach (var col in AdditionalColumns)
                {
                    if (string.IsNullOrWhiteSpace(col.ColumnName))
                        throw new ArgumentException("All custom columns must have a valid ColumnName property.");

                    if (col.DataType == SqlDataTypes.NotSupported)
                        throw new ArgumentException($"Column \"{col.ColumnName}\" specified an invalid or unsupported SQL column type.");

                    if (ClusteredColumnstoreIndex)
                        ColumnstoreCompatibilityCheck(col);
                }
            }

            // PK must always be NON-NULL
            if (PrimaryKey != null && PrimaryKey.AllowNull == true)
            {
                SelfLog.WriteLine($"Warning: Primary key must be NON-NULL, changing AllowNull property for {PrimaryKey.ColumnName} column.");
                PrimaryKey.AllowNull = false;
            }

            _configurationFinalized = true;
        }

        private static void ColumnstoreCompatibilityCheck(SqlColumn column)
        {
            if (!SqlDataTypes.ColumnstoreCompatible.Contains(column.DataType))
                throw new ArgumentException($"Columnstore indexes do not support data type \"{column.DataType}\" declared for column \"{column.ColumnName}\".");

            if (column.DataLength == -1 && SqlDataTypes.DataLengthRequired.Contains(column.DataType))
                SelfLog.WriteLine($"Warning: SQL2017 or newer required to use columnstore index with MAX length column \"{column.ColumnName}\".");
        }
    }
}
