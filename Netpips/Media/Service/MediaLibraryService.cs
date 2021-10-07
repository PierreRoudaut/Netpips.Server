using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;
using Netpips.Media.Filebot;
using Netpips.Media.Model;

namespace Netpips.Media.Service
{
    public class MediaLibraryService : IMediaLibraryService
    {
        private readonly ILogger<MediaLibraryService> logger;
        private readonly NetpipsSettings settings;
        private readonly IMediaLibraryMover mover;
        private readonly IFilebotService filebot;

        public MediaLibraryService(ILogger<MediaLibraryService> logger, IOptions<NetpipsSettings> appSettings, IMediaLibraryMover mover, IFilebotService filebot)
        {
            this.logger = logger;
            this.mover = mover;
            this.filebot = filebot;
            settings = appSettings.Value;
        }


        public IEnumerable<PlainMediaItem> AutoRename(PlainMediaItem item)
        {
            if (item.FileSystemInfo.IsDirectory())
            {
                logger.LogWarning("cannot autoRename: " + item.Path + " is a directory");
                return null;
            }

            return mover
                .MoveVideoFile(item.FileSystemInfo.FullName)
                .Select(fsInfo => new PlainMediaItem(fsInfo, settings.MediaLibraryPath));
        }

        public PlainMediaItem GetSubtitles(PlainMediaItem item, string lang)
        {
            if (item.FileSystemInfo.IsDirectory())
            {
                logger.LogWarning("cannot getSubtitles: " + item.Path + " is a directory");
                return null;
            }

            if (!filebot.GetSubtitles(item.FileSystemInfo.FullName, out var srtPath, lang))
            {
                logger.LogWarning("getSubtitles: " + item.Path + " subtitles not found");
                return null;
            }

            return new PlainMediaItem(new FileInfo(srtPath), settings.MediaLibraryPath);
        }
    }
}