using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Netpips.Download.Model;
using Netpips.Download.Service;

namespace Netpips.Download.Job
{
    public class ArchiveDownloadItemsJob : IInvocable
    {
        private readonly ILogger<ArchiveDownloadItemsJob> logger;

        private readonly IDownloadItemRepository repository;

        private readonly IDownloadItemService service;

        public const int ArchiveThresholdDays = 3;

        public ArchiveDownloadItemsJob(ILogger<ArchiveDownloadItemsJob> logger, IDownloadItemRepository repository, IDownloadItemService service)
        {
            this.logger = logger;
            this.repository = repository;
            this.service = service;
        }

        public Task Invoke()
        {
            logger.LogInformation("[ArchiveDownloadItemsJob] Start");
            var toArchive = repository.GetPassedItemsToArchive(ArchiveThresholdDays);
            logger.LogInformation($"[ArchiveDownloadItemsJob] {toArchive.Count} items to archive");
            toArchive.ForEach(item =>
            {
                service.ArchiveDownload(item);
                logger.LogInformation($"[ArchiveDownloadItemsJob] Archived {item.Token}");
            });

            logger.LogInformation($"[ArchiveDownloadItemsJob] Done");
            return Task.CompletedTask;
        }
    }
}