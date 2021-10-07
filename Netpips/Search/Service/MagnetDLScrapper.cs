using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Core.Http;
using Netpips.Search.Model;
using Python.Runtime;

namespace Netpips.Search.Service
{
    public class MagnetDLScrapper : BaseTorrentScrapper, ITorrentSearchScrapper, ITorrentDetailScrapper
    {
        private readonly ILogger<MagnetDLScrapper> logger;
        private static readonly Uri BaseUri = new Uri("https://www.magnetdl.com");
        private static readonly Uri TorrentDetailPrefixUri = new Uri(BaseUri, "file");
        protected override string SearchEndpointFormat => "http://magnetdl.com/{0}/{1}/";
        private readonly HttpClient httpClient;

        public MagnetDLScrapper(ILogger<MagnetDLScrapper> logger) : base(logger)
        {
            this.logger = logger;
            httpClient = new HttpClient(
                new HttpClientHandler
                {
                    AllowAutoRedirect = true
                })
            {
                BaseAddress = BaseUri,
                Timeout = TimeSpan.FromSeconds(3),
                DefaultRequestHeaders =
                {
                    Accept =
                    {
                        MediaTypeWithQualityHeaderValue.Parse("text/html")
                    }
                }

            };
        }


        public new async Task<string> ScrapeTorrentUrlAsync(string torrentDetailUrl)
        {
            await Task.Delay(0);
            var httpResponse = HttpCfscrapeService.GetFromPythonCommandLine(torrentDetailUrl);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var torrentUrl = ParseFirstMagnetLinkOrDefault(httpResponse.Html);
            return torrentUrl;

        }

        public new async Task<TorrentSearchResult> SearchAsync(string query)
        {
            await Task.Delay(0);
            // https://www.magnetdl.com/t/the-bad-batch-s01e09/

            var queryFirstLetter = query.Trim().ToLower().First();
            var queryAsKebabCase = query.Trim().ToLower().Replace(' ', '-').Replace("'", string.Empty);
            var searchUrl = $"https://www.magnetdl.com/{queryFirstLetter}/{queryAsKebabCase}/";

            var torrentSearchResult = new TorrentSearchResult
            {
                Response = HttpCfscrapeService.GetFromPythonCommandLine(searchUrl)
            };

            if (!torrentSearchResult.Response.IsSuccessStatusCode)
            {
                torrentSearchResult.Succeeded = false;
                return torrentSearchResult;
            }

            if (string.IsNullOrWhiteSpace(torrentSearchResult.Response.Html))
            {
                torrentSearchResult.Succeeded = false;
                return torrentSearchResult;
            }

            torrentSearchResult.Items = ParseTorrentSearchResult(torrentSearchResult.Response.Html);
            torrentSearchResult.Succeeded = true;
            
            return torrentSearchResult;
        }

        public override List<TorrentSearchItem> ParseTorrentSearchResult(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var items = htmlDocument.DocumentNode
                .Descendants("tbody").FirstOrDefault()
                ?.Elements("tr").Where(x => x.Elements("td").Count() == 8) // filter out blank rows
                .Select(tr =>
                {
                    var tds = tr.Elements("td").ToList();
                    var item = new TorrentSearchItem
                    {
                        TorrentUrl = HttpUtility.HtmlDecode(tds[0].Element("a").GetAttributeValue("href", string.Empty)),
                        Title =  tds[1].Element("a").GetAttributeValue("title", string.Empty),
                        ScrapeUrl = new Uri(BaseUri, tds[1].Element("a").GetAttributeValue("href", string.Empty))
                            .ToString(),
                        Size = ByteSize.TryParse(tds[5].InnerText.Trim(), out var size)
                            ? (long) Math.Round(size.Bytes)
                            : 0,
                        Seeders = int.TryParse(tds[6].InnerText, out var seeders)
                            ? seeders
                            : 0,
                        Leechers = int.TryParse(tds[7].InnerText, out var leechers)
                            ? leechers
                            : 0
                    };
                    return item;
                })
                .ToList();
            return items;
        }
        
        public bool CanScrape(Uri torrentDetailUri) => TorrentDetailPrefixUri.IsBaseOf(torrentDetailUri);
    }
}