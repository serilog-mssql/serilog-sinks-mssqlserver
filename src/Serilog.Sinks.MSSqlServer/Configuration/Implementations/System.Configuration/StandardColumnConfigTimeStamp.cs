using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class StandardColumnConfigTimeStamp : ColumnConfig
    {
        public StandardColumnConfigTimeStamp() : base()
        { }

        // override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = false, IsKey = true)]
        public override string ColumnName
        {
            get => base.ColumnName;
            set => base.ColumnName = value;
        }

        [ConfigurationProperty("ConvertToUtc")]
        public string ConvertToUtc
        {
            get => (string)base[nameof(ConvertToUtc)];
            set
            {
                base[nameof(ConvertToUtc)] = value;
            }
        }

    }
}

#pragma warning restore 1591

