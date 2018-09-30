using System;

namespace Serilog.Sinks.MSSqlServer.Tests
{
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

    public class TestTriggerEntry
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
    }
}
