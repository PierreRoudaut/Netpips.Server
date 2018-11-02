using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;

namespace Netpips.Download.DownloadMethod.PeerToPeer
{
    public class TransmissionRemoteDaemonService : ITorrentDaemonService
    {
        private string TrAuth => $"-n '{transmission.Username}:{transmission.Password}'";
        private readonly ILogger<TransmissionRemoteDaemonService> logger;
        private readonly NetpipsSettings settings;
        private readonly TransmissionSettings transmission;

        public TransmissionRemoteDaemonService(ILogger<TransmissionRemoteDaemonService> logger, IOptions<NetpipsSettings> options, IOptions<TransmissionSettings> transmission)
        {
            this.logger = logger;
            settings = options.Value;
            this.transmission = transmission.Value;
        }

        public bool AddTorrent(string torrentPath, string downloadDirPath)
        {
            var args = $"{TrAuth} -a " + torrentPath.Quoted() + " -w " + downloadDirPath.Quoted() + " --torrent-done-script " + settings.TorrentDoneScript.Quoted();
            logger.LogInformation("transmission-remote " + args);
            var result = OsHelper.ExecuteCommand("transmission-remote", args, out var output, out var error);
            logger.LogInformation($"output: {output} error: {error}");
            return result == 0;
        }

        public bool StopTorrent(string hash)
        {
            return OsHelper.ExecuteCommand("transmission-remote", $"{TrAuth} -t {hash} -S", out _, out _) == 0;
        }

        public bool RemoveTorrent(string hash)
        {
            return OsHelper.ExecuteCommand("transmission-remote", $"{TrAuth} -t {hash} -r", out _, out _) == 0;
        }

        public long GetDownloadedSize(string hash)
        {
            OsHelper.ExecuteCommand("transmission-remote", $"{TrAuth} -t {hash} -i", out var output, out var _);
            if (string.IsNullOrEmpty(output))
                return 0;
            return new TransmissionItem(output, null).DownloadedSize;

        }
    }
}