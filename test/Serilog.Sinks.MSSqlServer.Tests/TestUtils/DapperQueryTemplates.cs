using Serilog.Events;
using System;

namespace Serilog.Sinks.MSSqlServer.Tests.TestUtils
{
    public class CustomStandardLogColumns
    {
        public int CustomId { get; set; }
        public string CustomMessage { get; set; }
        public string CustomMessageTemplate { get; set; }
        public string CustomLevel { get; set; }
        public DateTime CustomTimeStamp { get; set; }
        public string CustomException { get; set; }
        public string CustomProperties { get; set; }
    }

    public class DefaultStandardLogColumns
    {
        public string Message { get; set; }
        public LogEventLevel Level { get; set; }
    }

    public class InfoSchema
    {
        public string ColumnName { get; set; }
        public string SchemaName { get; set; }
        public string DataType { get; set; }
    }

    public class sp_pkey
    {
        public string COLUMN_NAME { get; set; }
        public string PK_NAME { get; set; }
    }

    public class SysIndex_CCI
    {
        public string name { get; set; }
    }

    public class IdentityQuery
    {
        public int IsIdentity { get; set; }
    }

    public class LogEventColumn
    {
        public string LogEvent { get; set; }
    }

    public class SysObjectQuery
    {
        public int IndexType { get; set; }
    }

    public class PropertiesColumns
    {
        public string Properties { get; set; }
    }

    public class EnumLevelStandardLogColumns
    {
        public string Message { get; set; }
        public byte Level { get; set; }
    }

    public class StringLevelStandardLogColumns
    {
        public string Message { get; set; }
        public string Level { get; set; }
    }

    public class TestTimeStampDateTimeOffsetEntry
    {
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class TestTimeStampDateTimeEntry
    {
        public DateTime TimeStamp { get; set; }
    }

    public class TestTriggerEntry
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
    }
}
