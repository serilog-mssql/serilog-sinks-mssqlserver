using System;

namespace Serilog.Sinks.MSSqlServer.Tests
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
}