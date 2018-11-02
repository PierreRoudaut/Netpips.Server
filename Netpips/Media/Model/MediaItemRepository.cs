using System.Collections.Generic;
using System.IO;
using System.Linq;
using Humanizer;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;
using Netpips.Media.Service;

namespace Netpips.Media.Model
{
    public class MediaItemRepository : IMediaItemRepository
    {
        private readonly ILogger<MediaLibraryService> logger;

        private readonly NetpipsSettings settings;

        private readonly DirectoryInfo mediaLibraryDirInfo;

        public MediaItemRepository(ILogger<MediaLibraryService> logger, IOptions<NetpipsSettings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;
            mediaLibraryDirInfo = new DirectoryInfo(this.settings.MediaLibraryPath);
        }

        public IEnumerable<PlainMediaItem> FindAll()
        {
            var entries = mediaLibraryDirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
                .Select(fsInfo => new PlainMediaItem(fsInfo, settings.MediaLibraryPath)).OrderBy(x => x.Path);

            return entries;
        }

        public PlainMediaItem Find(string path)
        {
            var realPath = Path.GetFullPath(Path.Combine(settings.MediaLibraryPath, path));

            var fsInfo = mediaLibraryDirInfo
                .EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
                .FirstOrDefault(x => x.FullName == realPath);

            if (fsInfo == null)
            {
                return null;
            }

            return new PlainMediaItem(fsInfo, settings.MediaLibraryPath);
        }

        public IEnumerable<MediaFolderSummary> GetMediaLibraryRootFolderDistribution()
        {
            var groups = FindAll()
                .Where(x => !x.FileSystemInfo.IsDirectory())
                .GroupBy(x => x.Path.Split('/').First())
                .Select(
                    g => new MediaFolderSummary { Name = g.Key, Size = g.Sum(e => e.Size.GetValueOrDefault()) })
                .ToList();
            groups.ForEach(x => x.HumanizedSize = new ByteSize(x.Size).Humanize("#.##"));

            return groups;
        }

    }

    public class MediaFolderSummary
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string HumanizedSize { get; set; }
    }
}