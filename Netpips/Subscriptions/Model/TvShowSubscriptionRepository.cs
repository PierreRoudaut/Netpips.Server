using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Netpips.Download.Model;
using Netpips.Core.Model;

namespace Netpips.Subscriptions.Model
{
    public class TvShowSubscriptionRepository : ITvShowSubscriptionRepository
    {
        private readonly ILogger<TvShowSubscriptionRepository> logger;
        private readonly AppDbContext dbContext;

        public TvShowSubscriptionRepository(ILogger<TvShowSubscriptionRepository> logger, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public List<string> GetSubscribedUsersEmail(ShowRssItem showRssItem)
        {
            return dbContext.TvShowSubscriptions
                .Include(x => x.User)
                .Where(x => x.ShowRssId == showRssItem.ShowRssId && x.User.TvShowSubscriptionEmailNotificationEnabled)
                .Select(x => x.User.Email).ToList();
        }

        public bool IsSubscriptionDownload(DownloadItem item, out List<string> subscribedUsersEmail)
        {
            subscribedUsersEmail = null;
            var showRssItem = dbContext.ShowRssItems.FirstOrDefault(x => x.DownloadItemId == item.Id);
            if (showRssItem == null)
            {
                return false;
            }
            subscribedUsersEmail = dbContext.TvShowSubscriptions
                .Include(x => x.User)
                .Where(x => x.ShowRssId == showRssItem.ShowRssId && x.User.TvShowSubscriptionEmailNotificationEnabled)
                .Select(x => x.User.Email).ToList();
            return true;
        }
    }
}