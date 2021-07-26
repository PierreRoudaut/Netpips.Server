using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netpips.Core.Settings
{
    public class NetpipsSettings
    {
        public string HomePath { get; set; }
        public static string[] MediaFolders = { "TV Shows", "Movies", "Music", "Others", "Podcasts" };
        public IEnumerable<string> MediaFolderPaths => MediaFolders.Select(f => Path.Combine(MediaLibraryPath, f));
        public string MediaLibraryPath => Path.Combine(HomePath, "medialibrary");
        public string LogsPath => Path.Combine(HomePath, "logs");
        public string DownloadsPath => Path.Combine(HomePath, "downloads");
        public string TorrentDoneScript => Path.Combine(HomePath, ".torrent_done.sh");

        public string DaemonUserEmail { get; set; }

        public Uri Domain { get; set; }

        public Uri PlexDomain
        {
            get
            {
                var builder = new UriBuilder(Domain);
                builder.Host = "plex" + "." + builder.Host;
                builder.Scheme = "http";
                builder.Port = 80;
                return builder.Uri;
            }
        }
    }
}