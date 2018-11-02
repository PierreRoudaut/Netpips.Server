using System.Collections.Generic;
using Netpips.Download.Model;

namespace Netpips.Subscriptions.Model
{
    public interface ITvShowSubscriptionRepository
    {
        List<string> GetSubscribedUsersEmail(ShowRssItem item);

        bool IsSubscriptionDownload(DownloadItem item, out List<string> subscribedUsersEmail);
    }
}