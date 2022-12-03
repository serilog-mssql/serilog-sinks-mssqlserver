using static System.FormattableString;

namespace Serilog.Sinks.MSSqlServer.Extensions
{
    internal static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength, string suffix)
        {
            if (value == null) return null;
            var suffixLength = suffix?.Length ?? 0;
            if (maxLength <= suffixLength) return string.Empty;

            var correctedMaxLength = maxLength - suffixLength;
            return value.Length <= maxLength ? value : Invariant($"{value.Substring(0, correctedMaxLength)}{suffix}");
        }
    }
}
