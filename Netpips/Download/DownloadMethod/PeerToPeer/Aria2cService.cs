using System;
using Netpips.Core;
using Netpips.Core.Extensions;

namespace Netpips.Download.DownloadMethod.PeerToPeer
{
    public class Aria2CService : IAria2CService
    {
        public int DownloadTorrentFile(string magnetLink, string downloadFolder, TimeSpan? timeout = null)
        {
            return OsHelper.ExecuteCommand("aria2c",
                "--bt-metadata-only=true --bt-save-metadata=true -q " + magnetLink.Quoted() + " -d " +
                downloadFolder.Quoted(), out _, out _, timeout);
        }
    }
}
