using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Netpips.Core
{
    public static class FilesystemHelper
    {
        public static bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    return false;
                }
                using (File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("IsDirectoryWritable");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Ensure destFilename is deleted before sourceFileName is moved in order to avoid collision
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>

        public static void MoveOrReplace(string sourceFileName, string destFileName, ILogger<Object> logger = null)
        {
            if (sourceFileName == destFileName) return;
            try
            {
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }
                File.Move(sourceFileName, destFileName);

            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError("MoveOrReplace failed");
                    logger.LogError(ex.Message);
                }
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Ensure file or directory exists before getting deleted
        /// </summary>
        /// <param name="path"></param>

        public static void SafeDelete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return;
            }
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Creates parents directory of file if they do not exists
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        public static void SafeCopy(string fromPath, string toPath)
        {
            var dir = Path.GetDirectoryName(toPath);
            CreateDirectory(dir);
            File.Copy(fromPath, toPath);
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string ConvertToTwoLetterIsoLanguageNameSubtitle(string srtPath)
        {
            var tokens = srtPath.Split('.').Reverse().ToArray();
            var twoLetterIsoLanguageName = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .FirstOrDefault(c => c.ThreeLetterISOLanguageName == tokens[1].ToLower())
                ?.TwoLetterISOLanguageName;
            if (twoLetterIsoLanguageName == null)
            {
                return null;
            }
            tokens[1] = twoLetterIsoLanguageName;
            return string.Join('.', tokens.Reverse());
        }
    }
}