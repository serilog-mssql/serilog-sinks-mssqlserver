

namespace Serilog.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Collection of configuration items for use in generating DataColumn[]
    /// </summary>
    public class ColumnCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Create new element
        /// </summary>
        /// <returns>new ColumnConfig instance</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ColumnConfig();
        }

        /// <summary>
        /// Fetch Key for the Element
        /// </summary>
        /// <param name="element"></param>
        /// <returns>ColumnName</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ColumnConfig)element).ColumnName;
        }
    }
}
