using System;
using System.Collections.Generic;
using Netpips.Download.Model;

namespace Netpips.Subscriptions.Model
{
    public interface IShowRssItemRepository
    {
        void SyncFeedItems(List<ShowRssItem> items);

        ShowRssItem FindShowRssItem(Guid downloadItemId);

        ShowRssItem FindFirstQueuedItem();

        void Update(ShowRssItem item);

        /// <summary>
        /// Find completed items downloaded by a subscription on a rolling time window
        /// </summary>
        /// <param name="timeWindow">The number of past days to consider</param>
        /// <returns></returns>
        List<DownloadItem> FindRecentCompletedItems(int timeWindow);
    }
}