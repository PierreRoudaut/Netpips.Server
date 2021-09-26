using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Netpips.Core.Settings;

namespace Netpips.Core
{
    public static class AppAsserter
    {
        public static void AssertCliDependencies()
        {
            var commandList = new Dictionary<string, string> {
                // { "filebot", "-version" },
                // { "transmission-remote", "--version" },
                { "mediainfo", "--version" },
                // { "aria2c", "--version" }
            };
            foreach (var command in commandList)
            {
                var code = OsHelper.ExecuteCommand(command.Key, command.Value, out var output, out var error);
                if (code != 0 && code != 255)
                    throw new ApplicationException($@"[{command.Key}] code:[" + code + "]   out:[" + output + "]  error:[" + error + "]");
            }
        }

        public static void AssertSettings(NetpipsSettings settings)
        {
            new List<string> {
                settings.DownloadsPath,
                settings.LogsPath,
                settings.MediaLibraryPath,
            }
                .Concat(settings.MediaFolderPaths)
                .ToList()
                .ForEach(path =>
            {
                if (!FilesystemHelper.IsDirectoryWritable(path))
                    throw new ApplicationException($@"{path} is not writable");
            });
            if (!File.Exists(settings.TorrentDoneScript))
                throw new ApplicationException("Torrent done script: " + settings.TorrentDoneScript + " does not exists");
        }
    }
}