using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core;
using Netpips.Core.Settings;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace Netpips.Media.Service
{
    public class ArchiveExtractorService : IArchiveExtractorService
    {
        private readonly ILogger<ArchiveExtractorService> logger;

        private readonly NetpipsSettings settings;

        public ArchiveExtractorService(ILogger<ArchiveExtractorService> logger, IOptions<NetpipsSettings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }

        private bool ExtractArchive(RarArchive archive, string destinationDirectory)
        {
            var timer = Stopwatch.StartNew();
            var archiveName = Path.GetFileName(Path.GetDirectoryName(destinationDirectory));

            logger.LogInformation("sharpcompress ARCHIVE.EXTRACTION.STARTED [" + archiveName + "]");

            try
            {
                archive.Entries.Where(entry => !entry.IsDirectory).ToList().ForEach(
                    entry =>
                        {
                            var t = Stopwatch.StartNew();
                            logger.LogInformation("sharpcompress DECOMPRESSING [" + entry.Key + "]");
                            entry.WriteToDirectory(
                                destinationDirectory,
                                new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            t.Stop();
                            logger.LogInformation(
                                "sharpcompress DECOMPRESSED [" + entry.Key + "] in "
                                + TimeSpan.FromMilliseconds(t.ElapsedMilliseconds).TotalSeconds + "s");
                        });
            }
            catch (Exception e)
            {
                logger.LogError("sharpcompress ARCHIVE.EXTRACTION.FAILED [" + archiveName + "]");
                logger.LogError(e.Message);
                return false;
            }
            finally
            {
                archive.Dispose();
            }

            timer.Stop();
            logger.LogInformation("sharpcompress ARCHIVE.EXTRACTION.SUCCEEDED [" + archiveName + $"] {TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalSeconds}s");
            new DirectoryInfo(destinationDirectory).GetFiles("*.rar").ToList().ForEach(f => f.Delete());
            return true;
        }

        private string GetArchiveExtractionDirectory(string path, bool isMultiPart)
        {
            var destinationFolder = Path.GetFileNameWithoutExtension(path);
            var extractionDirectory = Path.Combine(Path.GetDirectoryName(path), destinationFolder);

            if (isMultiPart)
            {
                destinationFolder = string.Join('.', Path.GetFileName(path).Split('.').Reverse().Skip(2).Reverse());
                return Path.Combine(settings.MediaLibraryPath, "Others", destinationFolder);
            }

            return extractionDirectory;
        }

        private RarArchive GetFirstVolume(string movedPartArchivePath)
        {
            foreach (var entry in Directory.GetFiles(movedPartArchivePath).Where(RarArchive.IsRarFile))
            {
                var archive = RarArchive.Open(entry);
                if (IsValidFirstVolume(archive))
                {
                    return archive;
                }
                archive.Dispose();
            }

            return null;
        }

        private bool IsValidFirstVolume(RarArchive archive) =>
            archive.IsFirstVolume() && archive.IsComplete && archive.IsMultipartVolume();


        /// <summary>
        /// Handle a single or multipart rar file
        /// -Extract to the same folder if single volume
        /// -Moves the part.rar to the according extraction folder and triggers the extraction if archive is complete
        /// </summary>
        /// <param name="rarPath"></param>
        /// <param name="destDir"></param>
        /// <returns></returns>
        public bool HandleRarFile(string rarPath, out string destDir)
        {
            var archive = RarArchive.Open(rarPath);

            var isMultiPart = archive.IsMultipartVolume();
            var isComplete = archive.IsComplete;

            logger.LogInformation("HandleRarFile [" + Path.GetFileName(rarPath) + "] " + (isMultiPart ? "MULTIPART" : "SINGLEPART"));

            destDir = GetArchiveExtractionDirectory(rarPath, isMultiPart);
            FilesystemHelper.CreateDirectory(destDir);

            logger.LogInformation("HandleRarFile: extraction directory is [" + destDir + "]");

            // single rar file
            if (!isMultiPart && isComplete)
            {
                logger.LogInformation("HandleRarFile: extracting SINGLEPART");
                return ExtractArchive(archive, destDir);
            }
            archive.Dispose();

            // move part rar
            var movedPartArchivePath = Path.Combine(destDir, Path.GetFileName(rarPath));
            FilesystemHelper.MoveOrReplace(rarPath, movedPartArchivePath);
            logger.LogInformation("HandleRarFile: moved " + Path.GetFileName(rarPath) + " to extraction directory");
            logger.LogInformation("HandleRarFile: [" + destDir + "] : [" + string.Join(", ", Directory.GetFiles(destDir).Select(Path.GetFileName)) + "]");

            // find first volume of same archive and ensure archive is complete
            archive = GetFirstVolume(destDir);

            if (archive == null)
            {
                logger.LogWarning("HandleRarFile: archive INCOMPLETE");
                return false;
            }

            logger.LogInformation("HandleRarFile: archive COMPLETE, starting extraction");
            return ExtractArchive(archive, destDir);
        }
    }
}
