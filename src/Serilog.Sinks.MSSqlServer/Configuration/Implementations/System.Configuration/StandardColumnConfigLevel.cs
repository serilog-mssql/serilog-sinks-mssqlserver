using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class StandardColumnConfigLevel : ColumnConfig
    {
        public StandardColumnConfigLevel() : base()
        { }

        // override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = false, IsKey = true)]
        public override string ColumnName
        {
            get => base.ColumnName;
            set => base.ColumnName = value;
        }

        [ConfigurationProperty("StoreAsEnum")]
        public string StoreAsEnum
        {
            get => (string)base[nameof(StoreAsEnum)];
            set
            {
                base[nameof(StoreAsEnum)] = value;
            }
        }

    }
}

#pragma warning restore 1591

