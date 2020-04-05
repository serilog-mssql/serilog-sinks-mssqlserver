using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class StandardColumnCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new StandardColumnConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StandardColumnConfig)element)?.Name;
        }
    }
}

#pragma warning restore 1591
