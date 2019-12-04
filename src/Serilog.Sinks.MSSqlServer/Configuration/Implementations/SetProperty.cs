using System;

namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    /// Helper for applying only those properties actually specified in external configuration.
    /// </summary>
    public static partial class SetProperty
    {
        // Usage:
#pragma warning disable S125 // Sections of code should not be commented out
                            // SetProperty.IfValueNotNull<bool>(stringFromConfig, (boolOutputValue) => opts.BoolProperty = boolOutputValue);

        /// <summary>
        /// Simulates passing a property-setter to an "out" argument.
        /// </summary>
        public delegate void PropertySetter<in T>(T value);
#pragma warning restore S125 // Sections of code should not be commented out

        /// <summary>
        /// This will only set a value (execute the PropertySetter delegate) if the value is non-null.
        /// It also converts the provided value to the requested type. This allows configuration to only
        /// apply property changes when external configuration has actually provided a value.
        /// </summary>
        public static void IfNotNull<T>(string value, PropertySetter<T> setter)
        {
            if (value == null) return;
            try
            {
                T setting = (T)Convert.ChangeType(value, typeof(T));
                setter(setting);
            }
            // don't change the property if the conversion fails 
            catch (InvalidCastException) { }
            catch (OverflowException) { }
        }

        /// <summary>
        /// This will only set a value (execute the PropertySetter delegate) if the value is non-null and
        /// isn't empty or whitespace. This override is used when {T} is a string value that can't be empty.
        /// It also converts the provided value to the requested type. This allows configuration to only
        /// apply property changes when external configuration has actually provided a value.
        /// </summary>
        public static void IfNotNullOrEmpty<T>(string value, PropertySetter<T> setter)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            IfNotNull(value, setter);
        }
    }
}
