using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Identity.Model;
using Netpips.Subscriptions.Model;

namespace Netpips.Subscriptions.Job
{
    public class ShowRssFeedConsumerJob : IInvocable
    {
        private readonly ILogger<ShowRssFeedConsumerJob> logger;
        private readonly IShowRssItemRepository showRssItemRepository;
        private readonly IDownloadItemService downloadItemService;
        private readonly IUserRepository userRepository;
        public ShowRssFeedConsumerJob(ILogger<ShowRssFeedConsumerJob> logger, IShowRssItemRepository showRssItemRepository, IDownloadItemService downloadItemService, IUserRepository userRepository)
        {
            this.logger = logger;
            this.showRssItemRepository = showRssItemRepository;
            this.downloadItemService = downloadItemService;
            this.userRepository = userRepository;
        }

        public Task Invoke()
        {
            //todo: check if no download pendings
            logger.LogInformation("[ConsumeFeedJob] Start");
            var showRssItem = showRssItemRepository.FindFirstQueuedItem();
            if (showRssItem == null)
            {
                logger.LogInformation("[ConsumeFeedJob] 0 item to consume");
                return  Task.CompletedTask;
            }
            logger.LogInformation($"[ConsumeFeedJob] 1 item: {showRssItem.Title}");
            var daemonUser = userRepository.GetDaemonUser();
            var downloadItem = new DownloadItem
            {
                OwnerId = daemonUser.Id,
                FileUrl = showRssItem.Link
            };

            if (!downloadItemService.StartDownload(downloadItem, out var error))
            {
                logger.LogError("[ConsumeFeedJob] Failed: " + error);
                return Task.CompletedTask;
            }
            logger.LogInformation("[ConsumeFeedJob] Succeeded");
            showRssItem.DownloadItem = downloadItem;
            showRssItemRepository.Update(showRssItem);
            logger.LogInformation("[ConsumeFeedJob] Updated database");
            return Task.CompletedTask;
        }
    }
}