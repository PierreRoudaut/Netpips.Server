namespace Netpips.Search.Model
{
    public class TorrentSearchItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public int Leechers { get; set; }
        public int Seeders { get; set; }
        /// <summary>
        /// URL to scrape for magnet link
        /// </summary>
        public string ScrapeUrl { get; set; }
        /// <summary>
        /// Qualified torrent/magnet url if present in search results
        /// </summary>
        public string TorrentUrl { get; set; }
    }
}