using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Netpips.Subscriptions.Model
{
    public class TvShowRss : IEqualityComparer<TvShowRss>
    {
        public string ShowTitle { get; set; }
        public int ShowRssId { get; set; }

        [ExcludeFromCodeCoverage]
        public bool Equals(TvShowRss x, TvShowRss y)
        {
            return x.ShowTitle == y.ShowTitle && x.ShowRssId == y.ShowRssId;
        }

        [ExcludeFromCodeCoverage]
        public int GetHashCode(TvShowRss obj)
        {
            return obj.ShowRssId.GetHashCode() * obj.ShowTitle.GetHashCode();
        }
    }
}