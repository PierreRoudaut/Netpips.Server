using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Humanizer;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class MagnetDLScrapper : BaseTorrentScrapper, ITorrentSearchScrapper, ITorrentDetailScrapper
    {
        private readonly ILogger<MagnetDLScrapper> logger;
        private static readonly Uri BaseUri = new Uri("https://magnetdl.com");
        private static readonly Uri TorrentDetailPrefixUri = new Uri(BaseUri, "file");
        protected override string SearchEndpointFormat => "https://magnetdl.com/{0}/{1}/";
        private readonly HttpClient _httpClient;

        public MagnetDLScrapper(ILogger<MagnetDLScrapper> logger) : base(logger)
        {
            this.logger = logger;
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true
            });
        }

        private async Task<string> ProcessSearchQueryAsync(string query)
        {
            // https://www.magnetdl.com/t/the-bad-batch-s01e09/
            var firstPath = query.Trim().ToLower().First();
            var secondPath = query.Trim().ToLower().Replace(' ', '-').Replace("'", string.Empty);
            var url = string.Format(SearchEndpointFormat, firstPath, secondPath);

            logger.LogInformation("GET " + url);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers =
                {
                    Accept =
                    {
                        MediaTypeWithQualityHeaderValue.Parse("text/html")
                    }
                }
            };
            string html;
            try
            {
                var sw = Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request);
                html = await response.Content.ReadAsStringAsync();
                logger.LogInformation("{StatusCode} in {Ellapsed} {Bytes}", response.StatusCode.ToString("D"),
                    sw.ElapsedMilliseconds,
                    new ByteSize((double) response?.Content?.Headers?.ContentLength).Humanize("#"));
                if (response.IsSuccessStatusCode)
                {
                    logger.LogError(html);
                    return null;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occured");
                return null;
            }

            return html;
        }

        public async Task<List<TorrentSearchItem>> SearchAsync(string query)
        {
            var html = await ProcessSearchQueryAsync(query);
            if (!string.IsNullOrWhiteSpace(html))
                return null;

            // todo: unit test to parse magnetdl_search_results.html
            var items = ParseTorrentSearchResult(html);
            return items;
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
                        TorrentUrl = tds[0].Element("a").GetAttributeValue("href", string.Empty),
                        Title = tds[1].Element("a").GetAttributeValue("title", string.Empty),
                        ScrapeUrl =  new Uri(BaseUri, tds[1].Element("a").GetAttributeValue("href", string.Empty)).ToString(),
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