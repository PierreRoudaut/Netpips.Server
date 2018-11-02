using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Netpips.Download.Model;

namespace Netpips.Subscriptions.Model
{
    public class ShowRssItem : IEqualityComparer<ShowRssItem>
    {
        [Key]
        public string Guid { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime PubDate { get; set; }
        public int ShowRssId { get; set; }
        public int TvMazeShowId { get; set; }
        public string TvShowName { get; set; }
        public string Hash { get; set; }

        public DownloadItem DownloadItem { get; set; }
        public Guid? DownloadItemId { get; set; }

        [ExcludeFromCodeCoverage]
        public bool Equals(ShowRssItem x, ShowRssItem y)
        {
            return x.Guid == y.Guid;
        }

        [ExcludeFromCodeCoverage]
        public int GetHashCode(ShowRssItem obj)
        {
            return obj.Guid.GetHashCode();
        }
    }
}