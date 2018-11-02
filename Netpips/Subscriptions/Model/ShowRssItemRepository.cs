using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Netpips.Core.Model;

namespace Netpips.Subscriptions.Model
{
    public class ShowRssItemRepository : IShowRssItemRepository
    {
        private readonly ILogger<ShowRssItemRepository> logger;
        private readonly AppDbContext dbContext;

        public ShowRssItemRepository(ILogger<ShowRssItemRepository> logger, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public void SyncFeedItems(List<ShowRssItem> items)
        {
            var missingItems = items.Except(this.dbContext.ShowRssItems.ToList(), new ShowRssItem()).ToList();
            this.logger.LogInformation("[FeedSyncJob] " + missingItems.Count + " missing items");
            if (missingItems.Count == 0)
            {
                this.logger.LogInformation("[FeedSyncJob] No missing items to add");
                return;
            }
            try
            {
                this.dbContext.ShowRssItems.AddRange(missingItems);
                this.dbContext.SaveChanges();
                this.logger.LogInformation("[FeedSyncJob] Inserted " + missingItems.Count + " items");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError("[FeedSyncJob] Failed to Add missing items");
                logger.LogError(ex.Message);
            }
        }

        public ShowRssItem FindShowRssItem(Guid downloadItemId)
        {
            return this.dbContext.ShowRssItems.Include(x => x.DownloadItem).FirstOrDefault(x => x.DownloadItemId == downloadItemId);
        }

        public ShowRssItem FindFirstQueuedItem()
        {
            return this.dbContext.ShowRssItems
                .FirstOrDefault(x => x.DownloadItemId == null);
        }

        public void Update(ShowRssItem item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
    }
}