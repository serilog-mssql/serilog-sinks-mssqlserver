using System.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Class for reading System.Configuration (app.config, web.config) elements of the form &lt;PropertyName Value="Some value" /&gt;'
    /// </summary>
    public class ValueConfigElement : ConfigurationElement
    {
        /// <summary>
        /// Intiazes a new instance of ValueConfigElement
        /// </summary>
        public ValueConfigElement()
        {
        }

        /// <summary>
        /// Intiazes a new instance of ValueConfigElement with a value
        /// </summary>
        public ValueConfigElement(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The value property
        /// </summary>
        [ConfigurationProperty(nameof(Value), IsRequired = true)]
        public string Value
        {
            get { return (string)this[nameof(Value)]; }
            set { this[nameof(Value)] = value; }
        }
    }
}

