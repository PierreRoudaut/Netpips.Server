using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Settings;
using Netpips.Subscriptions.Model;

namespace Netpips.Subscriptions.Job
{
    public class ShowRssFeedSyncJob : IInvocable
    {
        private readonly ILogger<ShowRssFeedSyncJob> logger;

        private readonly IShowRssItemRepository repository;

        private readonly ShowRssSettings settings;

        public ShowRssFeedSyncJob(ILogger<ShowRssFeedSyncJob> logger, IOptions<ShowRssSettings> options, IShowRssItemRepository repository)
        {
            this.settings = options.Value;
            this.logger = logger;
            this.repository = repository;
        }

        public List<ShowRssItem> FetchItems()
        {
            //todo, make XElement load xml as Stream and bypass "unexpected token" error
            var feed = XElement.Load(this.settings.Feed);
            XNamespace np = feed.Attributes().First(a => a.Value.Contains("showrss")).Value;
            var items = feed
                .Descendants("item")
                .Select(
                    item => new ShowRssItem
                    {
                        Guid = item.Element("guid")?.Value,
                        Title = item.Element("title")?.Value,
                        Link = item.Element("link")?.Value,
                        ShowRssId = int.Parse(item.Element(np + "show_id")?.Value),
                        TvMazeShowId = int.Parse(item.Element(np + "external_id")?.Value),
                        Hash = item.Element(np + "info_hash")?.Value,
                        PubDate = DateTime.Parse(item.Element("pubDate")?.Value),
                        TvShowName = item.Element(np + "show_name")?.Value
                    }).ToList();


            return items;
        }

        public Task Invoke()
        {
            this.logger.LogInformation("[FeedSyncJob] Start");
            List<ShowRssItem> items;
            try
            {
                items = this.FetchItems();
                this.logger.LogInformation($"[FeedSyncJob] fetched {items.Count} items in feed");
            }
            catch (Exception ex)
            {
                this.logger.LogError("[FeedSyncJob] failed to fetch ShowRssItems");
                this.logger.LogError(ex.Message);
                return Task.CompletedTask;
            }
            this.repository.SyncFeedItems(items);
            this.logger.LogInformation("[FeedSyncJob] Completed");
            return Task.CompletedTask;
        }
    }
}