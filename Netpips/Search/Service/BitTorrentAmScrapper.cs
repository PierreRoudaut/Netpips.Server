
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class BitTorrentAmScrapper : BaseTorrentScrapper, ITorrentDetailScrapper, ITorrentSearchScrapper
    {
        public static Uri BaseUri = new Uri("https://bittorrent.am");

        public static Uri TorrentDetailPrefixUri = new Uri(BaseUri, "download-torrent");

        protected override string SearchEndpointFormat => "https://bittorrent.am/search.php?kwds={0}";

        public BitTorrentAmScrapper(ILogger<BaseTorrentScrapper> logger)
            : base(logger)
        {
        }

        public bool CanScrape(Uri torrentDetailUri) => TorrentDetailPrefixUri.IsBaseOf(torrentDetailUri);


        public override List<TorrentSearchItem> ParseTorrentSearchResult(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var items = htmlDocument
                .DocumentNode.Descendants().FirstOrDefault(n => n.Name == "table" && n.HasClass("torrentsTable"))
                ?.Descendants("tr").Where(tr => tr.HasClass("r"))
                .Select(tr =>
                {
                    var tds = tr.Descendants("td").ToList();
                    if (tds.Count < 6) return null;
                    var item = new TorrentSearchItem
                    {
                        Title = tds[2].ChildNodes[0].InnerText.Trim(),
                        ScrapeUrl = tds[2].ChildNodes[0].GetAttributeValue("href", null),
                        Size = ByteSize.TryParse(tds[3].FirstChild.InnerText.Trim(), out var size)
                                                  ? (long)size.Bytes
                                                  : 0,
                        Seeders = int.TryParse(tds[4].FirstChild.InnerText, out var seeders)
                                                     ? seeders
                                                     : 0,
                        Leechers = int.TryParse(tds[5].FirstChild.InnerText, out var leechers)
                                                      ? leechers
                                                      : 0
                    };
                    if (item.ScrapeUrl != null && item.Size > 0)
                    {
                        item.ScrapeUrl = BaseUri.OriginalString + item.ScrapeUrl;
                        return item;
                    }
                    return null;
                });
            return items != null ? items.Where(i => i != null).ToList() : new List<TorrentSearchItem>();
        }

    }
}
