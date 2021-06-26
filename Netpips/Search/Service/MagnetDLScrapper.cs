using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Humanizer;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class MagnetDLScrapper : BaseTorrentScrapper, ITorrentSearchScrapper
    {
        private readonly ILogger<MagnetDLScrapper> logger;
        public static Uri BaseUri = new Uri("https://1337x.to");
        
        protected override string SearchEndpointFormat => "https://magnetdl.com/{0}/{1}/";

        private readonly HttpClient _httpClient;
        
        public MagnetDLScrapper(ILogger<MagnetDLScrapper> logger) : base(logger)
        {
            this.logger = logger;
            _httpClient = new HttpClient();
        }

        private async Task<string> ProcessSearchQueryAsync(string query)
        {
            // https://www.magnetdl.com/t/the-bad-batch-s01e09/
            var firstPath = query.Trim().ToLower().First();
            var secondPath = query.Trim().ToLower().Replace(' ', '-').Replace("'", string.Empty);
            var url = string.Format(SearchEndpointFormat, firstPath, secondPath);

            Logger.LogInformation("GET " + url);
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
            HttpResponseMessage response = null;
            string html;
            try
            {
                var sw = Stopwatch.StartNew();
                response = await _httpClient.SendAsync(request);
                html = await response.Content.ReadAsStringAsync();
                logger.LogInformation("{StatusCode} in {Ellapsed} {Bytes}", response.StatusCode.ToString("D"), sw.ElapsedMilliseconds, new ByteSize((double) response?.Content?.Headers?.ContentLength).Humanize("#"));
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
                            ? (long) size.Bytes
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