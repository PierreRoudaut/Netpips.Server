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

        private async Task<TorrentSearchResult> ProcessSearchQueryAsync(string query)
        {
            // https://www.magnetdl.com/t/the-bad-batch-s01e09/

            var queryFirstLetter = query.Trim().ToLower().First();
            var queryAsKebabCase = query.Trim().ToLower().Replace(' ', '-').Replace("'", string.Empty);
            var path = $"{queryFirstLetter}/{queryAsKebabCase}/";

            var result = new TorrentSearchResult();

            logger.LogInformation("GET " + path);
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await httpClient.GetAsync(path);
                result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                result.HttpStatusCode = response.StatusCode;
                result.Html = await response.Content.ReadAsStringAsync();
                logger.LogInformation("{StatusCode} in {Ellapsed} {Bytes}", response.StatusCode.ToString("D"),
                    sw.ElapsedMilliseconds,
                    new ByteSize((double) response?.Content?.Headers?.ContentLength).Humanize("#"));
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError(result.Html);
                    result.Succeeded = false;
                    return result;
                }
            }
            catch (Exception e)
            {
                result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                logger.LogError(e, "An error occured");
                result.Succeeded = false;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public new async Task<TorrentSearchResult> SearchAsync(string query)
        {
            var result = await ProcessSearchQueryAsync(query);
            if (string.IsNullOrWhiteSpace(result.Html))
                return null;

            result.Items = ParseTorrentSearchResult(result.Html);
            return result;
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