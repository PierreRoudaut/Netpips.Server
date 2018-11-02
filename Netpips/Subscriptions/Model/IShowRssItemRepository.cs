using System;
using System.Collections.Generic;

namespace Netpips.Subscriptions.Model
{
    public interface IShowRssItemRepository
    {
        void SyncFeedItems(List<ShowRssItem> items);

        ShowRssItem FindShowRssItem(Guid downloadItemId);

        ShowRssItem FindFirstQueuedItem();

        void Update(ShowRssItem item);
    }
}