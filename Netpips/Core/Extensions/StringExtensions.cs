using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Netpips.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Prepend and append double quotes to string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Quoted(this string str) => '"' + str + '"';


        public static string ToSafeFilename(this string filename)
        {
            return filename.Replace("\"", string.Empty);
        }

        public static string ToCleanFileName(this string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static int ToInt(this string str)
        {
            return int.TryParse(str, out var i) ? i : default(int);
        }

        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
