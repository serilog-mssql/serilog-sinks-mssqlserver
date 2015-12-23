
namespace Serilog.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Settings configuration for defining DataColumns collection
    /// </summary>
    public class MSSqlServerConfigurationSection : ConfigurationSection
    {
        private static MSSqlServerConfigurationSection settings =
            ConfigurationManager.GetSection("MSSqlServerSettings") as MSSqlServerConfigurationSection;

        /// <summary>
        /// Access to the settings stored in the config file
        /// </summary>
        public static MSSqlServerConfigurationSection Settings
        {
            get
            {
                return settings;
            }
        }

        /// <summary>
        /// Columns in the database to write data into
        /// </summary>
        [ConfigurationProperty("Columns", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ColumnCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ColumnCollection Columns
        {
            get
            {
                return (ColumnCollection)base["Columns"];
            }
        }
    }
}
