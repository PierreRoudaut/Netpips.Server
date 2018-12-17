using System;
using System.Diagnostics;
using System.IO;
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
            this.logger.LogInformation("filebot " + args);
            var code = OsHelper.ExecuteCommand("filebot", args, out var output, out var error);
            var msg = $"code: {code}, output: {output}, error: {error}";
            this.logger.LogInformation(msg);
            Console.WriteLine(msg);

            /* -get-subtitles option always returns 0 regardless of failure */
            if (error.Contains("No matching subtitles found:"))
            {
                this.logger.LogWarning("filebot GetSubtitles FAILED output: " + error);
                return false;
            }
            var p = new Regex(@"Writing \[.*\] to \[(?<srt>.*)\]").Match(output);
            if (!p.Success || !p.Groups["srt"].Success)
            {
                this.logger.LogWarning("filebot GetSubtitles FAILED to capture destPath with Regex in output: " + output);
                return false;
            }
            srtPath = Path.Combine(Path.GetDirectoryName(path), p.Groups["srt"].Value);
            this.logger.LogInformation($"SUCCESS: {srtPath}");

            this.logger.LogInformation("Renaming to 2 letter iso code");
            var twoLetterSrtPath = FilesystemHelper.ConvertToTwoLetterIsoLanguageNameSubtitle(srtPath);
            if (twoLetterSrtPath != null)
            {
                FilesystemHelper.MoveOrReplace(srtPath, twoLetterSrtPath);
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