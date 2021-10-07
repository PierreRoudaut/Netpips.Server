using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Core.Http;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class _1337xScrapper : BaseTorrentScrapper, ITorrentDetailScrapper, ITorrentSearchScrapper
    {
        private static readonly Uri BaseUri = new Uri("https://1337x.to");

        private static readonly Uri TorrentDetailPrefixUri = new Uri(BaseUri, "torrent");

        protected override string SearchEndpointFormat => "https://1337x.to/search/{0}/1/";


        public _1337xScrapper(ILogger<_1337xScrapper> logger) : base(logger)
        {
        }

        public bool CanScrape(Uri torrentDetailUri) => TorrentDetailPrefixUri.IsBaseOf(torrentDetailUri);
        
        // public new async Task<string> ScrapeTorrentUrlAsync(string torrentDetailUrl)
        // {
        //     await Task.Delay(0);
        //     var httpResponse = HttpCfscrapeService.GetFromPythonCommandLine(torrentDetailUrl);
        //     
        //     if (!httpResponse.IsSuccessStatusCode)
        //     {
        //         return null;
        //     }
        //
        //     var torrentUrl = ParseFirstMagnetLinkOrDefault(httpResponse.Html);
        //     return torrentUrl;
        //
        // }

        public override List<TorrentSearchItem> ParseTorrentSearchResult(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var items = htmlDocument
                .DocumentNode.Descendants("tbody").FirstOrDefault()
                ?.Descendants("tr")
                .Select(tr =>
                    {
                        var tds = tr.Descendants("td").ToList();
                        if (tds.Count < 5) return null;
                        var item = new TorrentSearchItem
                                       {
                                           Title = tds[0].ChildNodes[1].InnerText.Trim(),
                                           ScrapeUrl = tds[0].ChildNodes[1].GetAttributeValue("href", null),
                                           Size = ByteSize.TryParse(tds[4].FirstChild.InnerText.Trim(), out var size) 
                                                      ? (long)size.Bytes 
                                                      : 0,
                                           Seeders = int.TryParse(tds[1].FirstChild.InnerText, out var seeders)
                                                         ? seeders
                                                         : 0,
                                           Leechers = int.TryParse(tds[2].FirstChild.InnerText, out var leechers)
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
