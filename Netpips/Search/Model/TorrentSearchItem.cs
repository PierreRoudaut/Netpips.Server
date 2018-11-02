namespace Netpips.Search.Model
{
    public class TorrentSearchItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public int Leechers { get; set; }
        public int Seeders { get; set; }
        public string ScrapeUrl { get; set; }
    }
}