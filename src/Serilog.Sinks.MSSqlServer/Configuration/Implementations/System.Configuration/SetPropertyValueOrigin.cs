using System.Configuration;

namespace Serilog.Sinks.MSSqlServer
{
    public static partial class SetProperty
    {
        // A null-check isn't possible for XML config strings; they default to an empty string and setting DefaultValue
        // to null doesn't work because internally the ConfigurationProperty attribute changes it back to empty string.

        /// <summary>
        /// Test the underlying property collection's value-origin flag for a non-default string value. Empty strings allowed.
        /// </summary>
        public static void IfProvided<T>(ConfigurationElement element, string propertyName, PropertySetter<T> setter)
        {
            if (element == null)
            {
                return;
            }

            var property = element.ElementInformation.Properties[propertyName];
            if (property.ValueOrigin == PropertyValueOrigin.Default) return;
            IfNotNull((string)property.Value, setter);
        }

        /// <summary>
        /// Test the underlying property collection's value-origin flag for a non-default, non-null, non-empty string value.
        /// </summary>
        public static void IfProvidedNotEmpty<T>(ConfigurationElement element, string propertyName, PropertySetter<T> setter)
        {
            if (element == null)
            {
                return;
            }

            var property = element.ElementInformation.Properties[propertyName];
            if (property.ValueOrigin == PropertyValueOrigin.Default) return;
            IfNotNullOrEmpty((string)property.Value, setter);
        }
    }
}
