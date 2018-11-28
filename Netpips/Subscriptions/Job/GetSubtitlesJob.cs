using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Options;
using Netpips.Core.Settings;
using Netpips.Media.Service;
using Netpips.Subscriptions.Model;
using Serilog;

namespace Netpips.Subscriptions.Job
{
    public class GetMissingSubtitlesJob : IInvocable
    {
        private readonly IShowRssItemRepository repository;
        private readonly IFilebotService filebot;
        private readonly NetpipsSettings settings;

        public GetMissingSubtitlesJob(IShowRssItemRepository repository, IFilebotService filebot, IOptions<NetpipsSettings> options)
        {
            this.repository = repository;
            this.filebot = filebot;
            this.settings = options.Value;
        }

        public Task Invoke()
        {
            var items = repository.FindCompletedItems(4);
            Log.Information($"[GetSubtitlesJob]: Found {items.Count} items needing subtitles");
            if (items.Count == 0) return Task.CompletedTask;

            foreach (var item in items)
            {
                var videoFileRelPath = item.MovedFiles.OrderByDescending(x => x.Size).FirstOrDefault()?.Path;
                Log.Information($"[GetSubtitlesJob] handling [{videoFileRelPath}]");
                var videoFullPath = Path.Combine(settings.MediaLibraryPath, videoFileRelPath);
                var fileInfo = new FileInfo(videoFullPath);
                if (!fileInfo.Exists || fileInfo.Directory == null)
                {
                    Log.Information($"[GetSubtitlesJob] cannot download missing subtitles for [{videoFileRelPath}] File has been moved");
                    continue;
                }

                var subs = fileInfo.Directory.EnumerateFiles("*.srt").ToList();
                if (subs.Count == 0)
                {
                    filebot.GetSubtitles(videoFullPath, out _, "en");
                    filebot.GetSubtitles(videoFullPath, out _, "fr");
                }
            }
            return Task.CompletedTask;
        }
    }
}
