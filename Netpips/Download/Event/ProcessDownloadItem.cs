using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Microsoft.Extensions.Logging;
using Netpips.Download.Model;
using Netpips.Media.Model;
using Netpips.Media.Service;

namespace Netpips.Download.Event
{
    public class ProcessDownloadItem : IListener<ItemDownloaded>
    {
        private readonly ILogger<ProcessDownloadItem> logger;
        private readonly IDownloadItemRepository repository;
        private readonly IMediaLibraryMover mediaLibraryMover;


        public ProcessDownloadItem(ILogger<ProcessDownloadItem> logger, IDownloadItemRepository repository, IMediaLibraryMover mediaLibraryMover)
        {
            this.logger = logger;
            this.repository = repository;
            this.mediaLibraryMover = mediaLibraryMover;
        }
        public Task HandleAsync(ItemDownloaded broadcasted)
        {
            this.logger.LogInformation("[HandleAsync] handling DownloadItemDownloaded event for: " + broadcasted.DownloadItemId);
            var item = repository.Find(broadcasted.DownloadItemId);

            // mark item as processing
            item = this.repository.Find(item.Id);
            item.DownloadedAt = DateTime.Now;
            item.State = DownloadState.Processing;
            repository.Update(item);

            logger.LogInformation($"[Processing] [{item.Name}]");
            var movedFiles = new List<MediaItem>();
            try
            {
                movedFiles = mediaLibraryMover.ProcessDownloadItem(item);
            }
            catch (System.Exception e)
            {
                this.logger.LogError("ProcessDownloadItem Failed to process download items");
                this.logger.LogError(e.Message);
            }

            // mark item as completed
            item = this.repository.Find(item.Id);
            item.MovedFiles = movedFiles;
            item.CompletedAt = DateTime.Now;
            item.State = DownloadState.Completed;
            repository.Update(item);
            return Task.CompletedTask;
        }
    }
}
