using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Humanizer;
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

        public async Task<HttpResponseLite> GetHttpResponseFomPythonCFScrapeAsync(string url)
        {
            if (!PythonEngine.IsInitialized)
                PythonEngine.Initialize();
            await Task.Delay(0);
            logger.LogInformation("GET " + url);
            var sw = Stopwatch.StartNew();
            var result = new HttpResponseLite();
            try
            {
                // https://github.com/pythonnet/pythonnet/wiki/Threading
                var mThreadState = PythonEngine.BeginAllowThreads();
                using (Py.GIL())
                {
                    dynamic cfscrape = Py.Import("cfscrape");
                    dynamic scraper = cfscrape.create_scraper();
                    PyObject response = scraper.get(url);
                    result.StatusCode = (HttpStatusCode) response.GetAttr("status_code").As<int>();
                    result.Html = response.GetAttr("text").As<string>();
                    result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                }
                PythonEngine.EndAllowThreads(mThreadState);
            }
            catch (Exception e)
            {
                result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                result.Exception = e;
            }
            return result;
        }
        
        private async Task<HttpResponseLite> GetHttpResponseAsync(string url)
        {
            var result = new HttpResponseLite();

            logger.LogInformation("GET " + url);
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await httpClient.GetAsync(url);
                result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                result.StatusCode = response.StatusCode;
                result.Html = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Ellapsed = $"{sw.ElapsedMilliseconds}ms";
                result.Exception = e;
                logger.LogError(e, "An error occured");
                return result;
            }
            return result;
        }

        public new async Task<string> ScrapeTorrentUrlAsync(string torrentDetailUrl)
        {
            var httpResponse = await GetHttpResponseFomPythonCFScrapeAsync(torrentDetailUrl);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var torrentUrl = ParseFirstMagnetLinkOrDefault(httpResponse.Html);
            return torrentUrl;

        }

        public new async Task<TorrentSearchResult> SearchAsync(string query)
        {
            // https://www.magnetdl.com/t/the-bad-batch-s01e09/

            var queryFirstLetter = query.Trim().ToLower().First();
            var queryAsKebabCase = query.Trim().ToLower().Replace(' ', '-').Replace("'", string.Empty);
            var searchUrl = $"https://www.magnetdl.com/{queryFirstLetter}/{queryAsKebabCase}/";

            var torrentSearchResult = new TorrentSearchResult
            {
                Response = await GetHttpResponseFomPythonCFScrapeAsync(searchUrl)
            };
            // var result = await ProcessSearchQueryAsync(searchUrl);

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