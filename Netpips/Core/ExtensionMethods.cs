using System;
using Microsoft.Extensions.Configuration;

namespace Netpips.Core
{
    public static class ExtensionMethods
    {
        public static T ToEnum<T>(this string value)
            where T : struct =>
            Enum.TryParse(value, true, out T result) ? result : default(T);

        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp).ToLocalTime();
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        public static T GetSectionSettings<T>(this IConfigurationRoot configurationRoot)
        {
            const string suffix = "Settings";

            var name = typeof(T).Name;
            var idx = name.IndexOf(suffix, StringComparison.Ordinal);
            if (!name.EndsWith(suffix) || idx == -1)
            {
                throw new Exception($"Type {name} is does not have Settings as a suffix");
            }
            var section = name.Substring(0, idx);
            var settings = configurationRoot.GetSection(section).Get<T>();
            return settings;
        }
    }
}