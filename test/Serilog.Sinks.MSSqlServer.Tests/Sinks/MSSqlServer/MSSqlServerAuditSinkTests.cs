using System;
using System.Collections.Generic;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Dependencies;
using Serilog.Sinks.MSSqlServer.Platform;
using Serilog.Sinks.MSSqlServer.Tests.TestUtils;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests
{
    [Trait(TestCategory.TraitName, TestCategory.Unit)]
    public class MSSqlServerAuditSinkTests : IDisposable
    {
        private readonly MSSqlServerSinkOptions _sinkOptions;
        private readonly Serilog.Sinks.MSSqlServer.ColumnOptions _columnOptions;
        private readonly SinkDependencies _sinkDependencies;
        private readonly Mock<ISqlCommandExecutor> _sqlDatabaseCreatorMock;
        private readonly Mock<ISqlCommandExecutor> _sqlTableCreatorMock;
        private readonly Mock<ISqlLogEventWriter> _sqlLogEventWriter;
        private readonly string _tableName = "tableName";
        private readonly string _schemaName = "schemaName";
        private MSSqlServerAuditSink _sut;
        private bool _disposedValue;

        public MSSqlServerAuditSinkTests()
        {
            _sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = _tableName,
                SchemaName = _schemaName
            };

            _columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            _sqlDatabaseCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlTableCreatorMock = new Mock<ISqlCommandExecutor>();
            _sqlLogEventWriter = new Mock<ISqlLogEventWriter>();

            _sinkDependencies = new SinkDependencies
            {
                SqlDatabaseCreator = _sqlDatabaseCreatorMock.Object,
                SqlTableCreator = _sqlTableCreatorMock.Object,
                SqlLogEventWriter = _sqlLogEventWriter.Object
            };
        }

        [Fact]
        public void InitializeWithoutTableNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerAuditSink(
                    new MSSqlServerSinkOptions(),
                    new Serilog.Sinks.MSSqlServer.ColumnOptions(),
                    _sinkDependencies));
        }

        [Fact]
        public void InitializeWithoutSinkDependenciesThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MSSqlServerSink(_sinkOptions, null));
        }

        [Fact]
        public void InitializeWithoutSqlTableCreatorThrows()
        {
            // Arrange
            _sinkDependencies.SqlTableCreator = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithoutSqlLogEventWriterThrows()
        {
            // Arrange
            _sinkDependencies.SqlLogEventWriter = null;

            // Act + assert
            Assert.Throws<InvalidOperationException>(() =>
                new MSSqlServerSink(_sinkOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithDisableTriggersThrows()
        {
            // Arrange
            var sinkOptions = new MSSqlServerSinkOptions { TableName = "TestTableName" };
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions { DisableTriggers = true };

            // Act + assert
            Assert.Throws<NotSupportedException>(() =>
                new MSSqlServerAuditSink(sinkOptions, columnOptions, _sinkDependencies));
        }

        [Fact]
        public void InitializeWithAutoCreateSqlDatabaseCallsSqlDatabaseCreator()
        {
            // Act
            SetupSut(autoCreateSqlDatabase: true);

            // Assert
            _sqlDatabaseCreatorMock.Verify(c => c.Execute(), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlDatabaseDoesNotCallSqlDatabaseCreator()
        {
            // Act
            SetupSut(autoCreateSqlDatabase: false);

            // Assert
            _sqlDatabaseCreatorMock.Verify(c => c.Execute(), Times.Never);
        }

        [Fact]
        public void InitializeWithAutoCreateSqlTableCallsSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: true);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.Execute(), Times.Once);
        }

        [Fact]
        public void InitializeWithoutAutoCreateSqlTableDoesNotCallSqlTableCreator()
        {
            // Act
            SetupSut(autoCreateSqlTable: false);

            // Assert
            _sqlTableCreatorMock.Verify(c => c.Execute(), Times.Never);
        }

        [Fact]
        public void EmitCallsSqlLogEventWriter()
        {
            // Arrange
            SetupSut();
            var logEvent = new Events.LogEvent(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Events.LogEventLevel.Information,
                null,
                new Events.MessageTemplate("", new List<MessageTemplateToken>()),
                new List<LogEventProperty>());

            // Act
            _sut.Emit(logEvent);

            // Assert
            _sqlLogEventWriter.Verify(w => w.WriteEvent(logEvent), Times.Once);
        }

        private void SetupSut(bool autoCreateSqlDatabase = false, bool autoCreateSqlTable = false)
        {
            _sinkOptions.AutoCreateSqlDatabase = autoCreateSqlDatabase;
            _sinkOptions.AutoCreateSqlTable = autoCreateSqlTable;
            _sut = new MSSqlServerAuditSink(_sinkOptions, _columnOptions, _sinkDependencies);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _sut?.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
