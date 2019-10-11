using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class StandardColumnConfigLogEvent : ColumnConfig
    {
        public StandardColumnConfigLogEvent() : base()
        { }

        // override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = false, IsKey = true)]
        public override string ColumnName
        {
            get => base.ColumnName;
            set => base.ColumnName = value;
        }

        [ConfigurationProperty("ExcludeAdditionalProperties")]
        public string ExcludeAdditionalProperties
        {
            get => (string)base["ExcludeAdditionalProperties"];
            set
            {
                base["ExcludeAdditionalProperties"] = value;
            }
        }

        [ConfigurationProperty("ExcludeStandardColumns")]
        public string ExcludeStandardColumns
        {
            get => (string)base["ExcludeStandardColumns"];
            set
            {
                base["ExcludeStandardColumns"] = value;
            }
        }
    }
}

#pragma warning restore 1591

