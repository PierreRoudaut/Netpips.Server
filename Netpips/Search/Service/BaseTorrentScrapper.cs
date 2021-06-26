using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Humanizer;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Netpips.Core;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public abstract class BaseTorrentScrapper
    {
        protected readonly ILogger<BaseTorrentScrapper> Logger;
        protected abstract string SearchEndpointFormat { get; }


        protected BaseTorrentScrapper(ILogger<BaseTorrentScrapper> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Fetch html
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> DoGet(string url, string acceptRequestHeaderValue = null)
        {
            Logger.LogInformation("GET " + url);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
                if (!string.IsNullOrWhiteSpace(acceptRequestHeaderValue))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptRequestHeaderValue));
                }
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response;
                try
                {
                    response = await client.SendAsync(request);
                }
                catch (Exception e)
                {
                    Logger.LogWarning(url + " request failed");
                    Logger.LogWarning(e.Message);
                    return null;
                }
                Logger.LogInformation("GET " + url + " HttpStatusCode " + response.StatusCode.ToString("D"));
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning("GET " + url + " request failed");
                    return null;
                }
                var html = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(html))
                {
                    Logger.LogWarning("GET " + url + " empty response");
                    return null;
                }
                Logger.LogInformation("GET " + url + " " + new ByteSize(html.Length).Humanize("#"));
                return html;
            }
        }

        public async Task<string> ScrapeTorrentUrlAsync(string torrentDetailUrl)
        {
            var html = await DoGet(torrentDetailUrl);
            if (html == null)
            {
                return null;
            }

            var torrentUrl = ParseTorrentDetailResult(html);
            return torrentUrl;
        }

        public string ParseTorrentDetailResult(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var a = htmlDocument.DocumentNode.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", "").StartsWith("magnet"));

            return a?.Attributes["href"].Value;
        }


        // SEARCH

        public async Task<List<TorrentSearchItem>> SearchAsync(string query)
        {
            var urlEndpoint = string.Format(SearchEndpointFormat, HttpUtility.UrlEncode(query.ToLower()));
            var html = await DoGet(urlEndpoint);
            if (html == null)
            {
                return new List<TorrentSearchItem>();
            }

            var items = ParseTorrentSearchResult(html);
            return items;
        }

        public abstract List<TorrentSearchItem> ParseTorrentSearchResult(string html);
    }
}