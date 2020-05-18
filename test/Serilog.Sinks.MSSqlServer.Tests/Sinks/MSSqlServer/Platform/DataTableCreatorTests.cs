using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Platform
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class DataTableCreatorTests
    {
        private const string _tableName = "TestTableName";
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private DataTableCreator _sut;

        public DataTableCreatorTests()
        {
            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
        }

        [Fact]
        public void InitializeThrowsIfTableNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DataTableCreator(null, _columnOptions));
        }

        [Fact]
        public void InitializeThrowsIfColumnOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DataTableCreator(_tableName, null));
        }

        [Fact]
        public void CreateDataTableAddsStandardColumns()
        {
            // Arrange
            _columnOptions.Store.Add(StandardColumn.LogEvent);
            SetupSut();

            // Act
            var result = _sut.CreateDataTable();

            // Assert
            Assert.Equal(_columnOptions.Store.Count, result.Columns.Count);
            CompareStandardColumn(result, StandardColumn.Id, nameof(StandardColumn.Id));
            CompareStandardColumn(result, StandardColumn.Message, nameof(StandardColumn.Message));
            CompareStandardColumn(result, StandardColumn.MessageTemplate, nameof(StandardColumn.MessageTemplate));
            CompareStandardColumn(result, StandardColumn.Level, nameof(StandardColumn.Level));
            CompareStandardColumn(result, StandardColumn.TimeStamp, nameof(StandardColumn.TimeStamp));
            CompareStandardColumn(result, StandardColumn.Exception, nameof(StandardColumn.Exception));
            CompareStandardColumn(result, StandardColumn.Properties, nameof(StandardColumn.Properties));
            CompareStandardColumn(result, StandardColumn.LogEvent, nameof(StandardColumn.LogEvent));
        }

        [Fact]
        public void CreateDataTableSetsCustomStandardColumnAsPrimaryKey()
        {
            // Arrange
            var messageColumn = _columnOptions.GetStandardColumnOptions(StandardColumn.Message);
            _columnOptions.PrimaryKey = messageColumn;
            SetupSut();

            // Act
            var result = _sut.CreateDataTable();

            // Assert
            messageColumn.AllowNull = false; // If column was made PK, AllowNulls was set to false.
            var messageDataColumn = messageColumn.AsDataColumn();
            AssertColumnsEqual(messageDataColumn, result.PrimaryKey.Single());
        }

        [Fact]
        public void CreateDataTableAddsAdditionalColumns()
        {
            // Arrange
            _columnOptions.Store.Remove(StandardColumn.Id);
            _columnOptions.Store.Remove(StandardColumn.Message);
            _columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            _columnOptions.Store.Remove(StandardColumn.Level);
            _columnOptions.Store.Remove(StandardColumn.TimeStamp);
            _columnOptions.Store.Remove(StandardColumn.Exception);
            _columnOptions.Store.Remove(StandardColumn.Properties);
            var additionalColumn1 = new SqlColumn { ColumnName = "AdditionalColumn1", AllowNull = false, DataType = SqlDbType.BigInt };
            var additionalColumn2 = new SqlColumn { ColumnName = "AdditionalColumn2", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 10 };
            _columnOptions.AdditionalColumns = new List<SqlColumn> { additionalColumn1, additionalColumn2 };
            SetupSut();

            // Act
            var result = _sut.CreateDataTable();

            // Assert
            Assert.Equal(2, result.Columns.Count);
            AssertColumnsEqual(additionalColumn1.AsDataColumn(), result.Columns["AdditionalColumn1"]);
            AssertColumnsEqual(additionalColumn2.AsDataColumn(), result.Columns["AdditionalColumn2"]);
        }

        [Fact]
        public void CreateDataTableSetsCustomAdditionalColumnAsPrimaryKey()
        {
            // Arrange
            _columnOptions.Store.Remove(StandardColumn.Id);
            _columnOptions.Store.Remove(StandardColumn.Message);
            _columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            _columnOptions.Store.Remove(StandardColumn.Level);
            _columnOptions.Store.Remove(StandardColumn.TimeStamp);
            _columnOptions.Store.Remove(StandardColumn.Exception);
            _columnOptions.Store.Remove(StandardColumn.Properties);
            var additionalColumn1 = new SqlColumn { ColumnName = "AdditionalColumn1", AllowNull = false, DataType = SqlDbType.BigInt };
            var additionalColumn2 = new SqlColumn { ColumnName = "AdditionalColumn2", AllowNull = true, DataType = SqlDbType.NVarChar, DataLength = 10 };
            _columnOptions.AdditionalColumns = new List<SqlColumn> { additionalColumn1, additionalColumn2 };
            _columnOptions.PrimaryKey = additionalColumn1;
            SetupSut();

            // Act
            var result = _sut.CreateDataTable();

            // Assert
            var additionalColumn1DataColumn = additionalColumn1.AsDataColumn();
            AssertColumnsEqual(additionalColumn1DataColumn, result.PrimaryKey.Single());
        }

        private void SetupSut()
        {
            _sut = new DataTableCreator(_tableName, _columnOptions);
        }

        private static void AssertColumnsEqual(DataColumn column, DataColumn dataColumn)
        {
            Assert.Equal(column.ColumnName, dataColumn.ColumnName);
            Assert.Equal(column.DataType, dataColumn.DataType);
            Assert.Equal(column.MaxLength, dataColumn.MaxLength);
            Assert.Equal(column.AllowDBNull, dataColumn.AllowDBNull);
        }

        private void CompareStandardColumn(DataTable result, StandardColumn standardColumn, string standardColumnName)
        {
            var column = _columnOptions.GetStandardColumnOptions(standardColumn).AsDataColumn();
            var dataColumn = result.Columns[standardColumnName];
            AssertColumnsEqual(column, dataColumn);
        }
    }
}
