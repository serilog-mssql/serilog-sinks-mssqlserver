using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Extensions
{
    internal static class StringExtensions
    {
        public static string TruncateOutput(this string value, int dataLength) =>
            dataLength < 0
                ? value     // No need to truncate if length set to maximum
                : value.Truncate(dataLength, "...");

        public static string Truncate(this string value, int maxLength, string suffix)
        {
            if (value == null) return null;
            else if (value.Length <= maxLength) return value;

            var suffixLength = suffix?.Length ?? 0;
            if (maxLength <= suffixLength) return string.Empty;

            var correctedMaxLength = maxLength - suffixLength;
            return Invariant($"{value.Substring(0, correctedMaxLength)}{suffix}");
        }
    }
}
