﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Netpips.Core.Model;
using Netpips.Download.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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


        /// <inheritdoc />
        public void SyncFeedItems(List<ShowRssItem> items)
        {
            var missingItems = items.Except(dbContext.ShowRssItems.ToList(), new ShowRssItem()).ToList();
            logger.LogInformation("[FeedSyncJob] " + missingItems.Count + " missing items");
            if (missingItems.Count == 0)
            {
                logger.LogInformation("[FeedSyncJob] No missing items to add");
                return;
            }
            try
            {
                dbContext.ShowRssItems.AddRange(missingItems);
                dbContext.SaveChanges();
                logger.LogInformation("[FeedSyncJob] Inserted " + missingItems.Count + " items");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError("[FeedSyncJob] Failed to Add missing items");
                logger.LogError(ex.Message);
            }
        }

        public ShowRssItem FindShowRssItem(Guid downloadItemId)
        {
            return dbContext.ShowRssItems.Include(x => x.DownloadItem).FirstOrDefault(x => x.DownloadItemId == downloadItemId);
        }

        public ShowRssItem FindFirstQueuedItem()
        {
            return dbContext.ShowRssItems
                .FirstOrDefault(x => x.DownloadItemId == null);
        }

        public void Update(ShowRssItem item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public List<DownloadItem> FindRecentCompletedItems(int timeWindow)
        {
            var threshold = DateTime.Now.AddDays(-timeWindow);
            var items = dbContext
                .ShowRssItems
                .Include(x => x.DownloadItem)
                .Where(s => s.DownloadItem != null && s.DownloadItem.State == DownloadState.Completed &&
                            s.DownloadItem.CompletedAt >= threshold)
                .Select(s => s.DownloadItem)
                .ToList();
            return items;
        }
    }
}