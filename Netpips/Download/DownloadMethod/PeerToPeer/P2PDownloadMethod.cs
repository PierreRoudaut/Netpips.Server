using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using BencodeNET.Parsing;
using BencodeNET.Torrents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core;
using Netpips.Core.Settings;
using Netpips.Download.Exception;
using Netpips.Download.Model;

namespace Netpips.Download.DownloadMethod.PeerToPeer
{
    public class P2PDownloadMethod : IDownloadMethod
    {

        private readonly ITorrentDaemonService torrentDaemonService;
        private readonly NetpipsSettings settings;
        private readonly IAria2CService aria2CService;
        private readonly ILogger<P2PDownloadMethod> logger;

        public P2PDownloadMethod(ILogger<P2PDownloadMethod> logger, IOptions<NetpipsSettings> settings, IAria2CService aria2CService, ITorrentDaemonService torrentDaemonService)
        {
            this.torrentDaemonService = torrentDaemonService;
            this.logger = logger;
            this.aria2CService = aria2CService;
            this.settings = settings.Value;
        }

        public bool Cancel(DownloadItem item) => torrentDaemonService.StopTorrent(item.Hash);


        public long GetDownloadedSize(DownloadItem item) => torrentDaemonService.GetDownloadedSize(item.Hash);

        public const string NoPeersFoundMessage = "No peers found for torrent";
        public const string BrokenMagnetLinkMessage = "Magnet link seems to be broken";
        public const string TorrentFileNotFoundMessage = "Torrent file could not be downloaded";
        public const string TorrentFileCorrupted = "Torrent file might be corrupted";
        public const string TorrentDaemonAddFailureMessage = "Failed to add torrent to engined";


        /// <summary>
        /// Transforms a magnet link into a torrent file
        /// </summary>
        /// <param name="magnetLink"></param>
        /// <param name="torrentFolder"></param>
        /// <exception cref="FileNotDownloadableException">If no peers were found in the timeout</exception>
        private void DownloadMagnetLink(string magnetLink, string torrentFolder)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var exitCode = aria2CService.DownloadTorrentFile(magnetLink, torrentFolder, TimeSpan.FromSeconds(120));
            stopWatch.Stop();
            logger.LogInformation("aria2c duration: " + stopWatch.Elapsed.Seconds + "," + stopWatch.Elapsed.Milliseconds + "s");
            if (exitCode == -1)
            {
                throw new FileNotDownloadableException(NoPeersFoundMessage);
            }
            if (exitCode != 0)
            {
                throw new FileNotDownloadableException(BrokenMagnetLinkMessage);
            }
        }

        /// <summary>
        /// Download a torrent file given url into a given download torrent folder
        /// </summary>
        /// <param name="torrentUrl"></param>
        /// <param name="torrentFolder"></param>
        /// <exception cref="FileNotDownloadableException">If the file could not be downloadable</exception>
        private void DownloadTorrentFile(string torrentUrl, string torrentFolder)
        {
            using (var client = new WebClient())
            {
                try
                {
                    logger.LogInformation("Downloading torrent url: " + torrentUrl);
                    client.DownloadFile(new Uri(torrentUrl), Path.Combine(torrentFolder, Path.GetRandomFileName() + ".torrent"));
                    logger.LogInformation("SUCCESS");
                }
                catch (System.Exception)
                {
                    logger.LogInformation("FAILURE torrent url could not be downloaded");
                    throw new FileNotDownloadableException(TorrentFileNotFoundMessage);
                }
            }
        }


        /// <summary>
        /// Returns a parsed torrent
        /// </summary>
        /// <param name="torrentPath"></param>
        /// <exception cref="FileNotDownloadableException">if torrent is corrupted</exception>
        /// <returns></returns>
        private Torrent ParseTorrent(string torrentPath)
        {
            try
            {
                return new BencodeParser().Parse<Torrent>(torrentPath);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(TorrentFileCorrupted + " ex: " + ex.Message);
                throw new FileNotDownloadableException(TorrentFileCorrupted);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Tries to start a P2P Download
        /// If the file is a magnet, it'll be converted into a torrent using aria2c CLI
        /// If the file is a torrent url, it'll download the torrent file locally
        /// Parses and assert the torrent validity before adding the torrent using transmission-remote CLI
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Start(DownloadItem item)
        {
            item.Type = DownloadType.P2P;
            item.Token = "_" + DownloadType.P2P.ToString().ToLower() + Guid.NewGuid().ToString("N");

            var itemFolder = Path.Combine(settings.DownloadsPath, item.Token);
            Directory.CreateDirectory(itemFolder);


            var torrentPath = "";
            Torrent torrent;
            try
            {
                //Download torrent file 
                (item.FileUrl.StartsWith("magnet:?")
                        ? DownloadMagnetLink
                        : (Action<string, string>)DownloadTorrentFile)
                    (item.FileUrl, itemFolder);

                //Rename torrent file
                var tPath = new DirectoryInfo(itemFolder).EnumerateFiles("*.torrent").FirstOrDefault()?.FullName;
                torrentPath = itemFolder + ".torrent";
                FilesystemHelper.MoveOrReplace(tPath, torrentPath);

                //Parse torrent info
                torrent = ParseTorrent(torrentPath);

                //Add to torrent daemon service
                if (!torrentDaemonService.AddTorrent(torrentPath, itemFolder))
                    throw new StartDownloadException(TorrentDaemonAddFailureMessage);
            }
            catch (System.Exception)
            {
                FilesystemHelper.SafeDelete(itemFolder);
                throw;
            }
            finally
            {
                FilesystemHelper.SafeDelete(torrentPath);
            }

            item.Name = torrent.DisplayName;
            item.Hash = torrent.OriginalInfoHash.ToLower();
            item.TotalSize = torrent.TotalSize;
        }

        public bool Archive(DownloadItem item)
        {
            return torrentDaemonService.RemoveTorrent(item.Hash);
        }

        public static List<Regex> SupportedUrls = new List<Regex>
        {
            new Regex(@"magnet:?.*")
        };

        private bool PointsToTorrentUrl(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
                HttpResponseMessage response;
                try
                {
                    response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                }
                catch
                {
                    return false;
                }
                return response.Content.Headers?.ContentDisposition?.FileName?.TrimStart('"')?.TrimEnd('"')?.EndsWith(".torrent") ?? false;
            }
        }

        /// <summary>
        /// Check if a given url points to a valid P2P resource
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public bool CanHandle(string fileUrl)
        {
            var res = SupportedUrls.Any(x => x.IsMatch(fileUrl));
            if (res)
                return true;
            if (Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute))
                return PointsToTorrentUrl(fileUrl);
            return false;
        }

        public bool CanHandle(DownloadType type)
        {
            return type == DownloadType.P2P;
        }
    }
}