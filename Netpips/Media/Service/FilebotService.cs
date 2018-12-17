using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Netpips.Core;
using Netpips.Core.Extensions;

namespace Netpips.Media.Service
{
    public class FilebotService : IFilebotService
    {
        private readonly ILogger<IFilebotService> logger;

        private static readonly Regex FileAlreadyExistsPattern = new Regex(@"because \[(?<dest>.*)\] already exists");

        public FilebotService(ILogger<IFilebotService> logger)
        {
            this.logger = logger;
        }

        public bool GetSubtitles(string path, out string srtPath, string lang = "eng", bool nonStrict = false)
        {
            srtPath = "";
            var args = "-get-subtitles " + path.Quoted() + " --lang " + lang.Quoted();
            if (nonStrict)
            {
                args += " -non-strict ";
            }

            var expectedSrtPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + $".{lang}.srt";
            this.logger.LogInformation("filebot " + args);
            Console.WriteLine("filebot " + args);

            var code = OsHelper.ExecuteCommand("filebot", args, out var output, out var error);
            var msg = $"code: {code}, output: {output}, error: {error}";
            Console.WriteLine(msg);
            this.logger.LogInformation(msg);
            if (!File.Exists(expectedSrtPath))
            {
                return false;
            }

            /* -get-subtitles option always returns 0 regardless of failure */
            this.logger.LogInformation("Renaming to 2 letter iso code");
            var twoLetterSrtPath = FilesystemHelper.ConvertToTwoLetterIsoLanguageNameSubtitle(expectedSrtPath);
            if (twoLetterSrtPath != null)
            {
                FilesystemHelper.MoveOrReplace(expectedSrtPath, twoLetterSrtPath);
                srtPath = twoLetterSrtPath;
            }
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Get the media location for the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseDestPath"></param>
        /// <param name="destPath"></param>
        /// <param name="db"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool TryRename(string path, string baseDestPath, out string destPath, string db = null, string action = "test")
        {
            destPath = "";
            var destFormat = baseDestPath + Path.DirectorySeparatorChar + "{plex}";
            var args = "-rename " + path.Quoted() + " --format " + destFormat.Quoted() + " -non-strict --action " + action.Quoted();
            if (db != null)
            {
                args += " --db " + db.Quoted();
            }

            this.logger.LogInformation("filebot " + args);
            var exitCode = OsHelper.ExecuteCommand("filebot", args, out var output, out var error);
            this.logger.LogInformation($"code: {exitCode}, output: {output}, error: {error}");

            if (exitCode != 0)
            {
                var match = FileAlreadyExistsPattern.Match(output);
                if (match.Success && match.Groups["dest"].Success)
                {
                    destPath = match.Groups["dest"].Value;
                    this.logger.LogInformation($"Filebot.TryRename [SUCCESS] [FileAlreadyExists] [{destPath}]");
                    return true;
                }
                this.logger.LogWarning("Filebot.TryRename [FAILED]", args, error);
                return false;
            }
            var p = new Regex(@"\[" + action.ToUpper() + @"\].*\[.*\] to \[(?<dest>.*)\]").Match(output);
            if (p.Success && p.Groups["dest"].Success)
            {
                destPath = p.Groups["dest"].Value;
                this.logger.LogWarning($"Filebot.TryRename [SUCCESS] [{destPath}]");
                return true;
            }
            this.logger.LogWarning("Filebot.TryRename [FAILED] to capture destPath in output: ", output);
            return false;
        }
    }
}