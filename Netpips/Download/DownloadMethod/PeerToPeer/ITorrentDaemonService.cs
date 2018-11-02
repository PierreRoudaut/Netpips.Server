namespace Netpips.Download.DownloadMethod.PeerToPeer
{
    public interface ITorrentDaemonService
    {
        bool AddTorrent(string torrentPath, string downloadDirPath);
        bool StopTorrent(string hash);
        bool RemoveTorrent(string hash);
        long GetDownloadedSize(string hash);
    }
}