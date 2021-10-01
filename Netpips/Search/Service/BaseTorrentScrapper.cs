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
using Netpips.Core.Http;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public abstract class BaseTorrentScrapper
    {
        private readonly ILogger<BaseTorrentScrapper> logger;
        protected abstract string SearchEndpointFormat { get; }


        protected BaseTorrentScrapper(ILogger<BaseTorrentScrapper> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Fetch html
        /// </summary>
        /// <param name="url"></param>
        /// <param name="acceptRequestHeaderValue"></param>
        /// <returns></returns>
        public async Task<string> DoGet(string url, string acceptRequestHeaderValue = null)
        {
            logger.LogInformation("GET " + url);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/html");
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
                    logger.LogWarning(url + " request failed");
                    logger.LogWarning(e.Message);
                    return null;
                }
                logger.LogInformation("GET " + url + " HttpStatusCode " + response.StatusCode.ToString("D"));
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("GET " + url + " request failed");
                    return null;
                }
                var html = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(html))
                {
                    logger.LogWarning("GET " + url + " empty response");
                    return null;
                }
                logger.LogInformation("GET " + url + " " + new ByteSize(html.Length).Humanize("#"));
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

            var torrentUrl = ParseFirstMagnetLinkOrDefault(html);
            return torrentUrl;
        }

        
        /// <summary>
        /// Retrieves the first found magnet link on a page
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string ParseFirstMagnetLinkOrDefault(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var a = htmlDocument.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("href", string.Empty).StartsWith("magnet:?"));

            var magnetLink = a?.GetAttributeValue("href", null);
            return !string.IsNullOrWhiteSpace(magnetLink) ? HttpUtility.HtmlDecode(magnetLink) : null;
        }


        public async Task<TorrentSearchResult> SearchAsync(string query)
        {
            var urlEndpoint = string.Format(SearchEndpointFormat, HttpUtility.UrlEncode(query.ToLower()));
            var result = new TorrentSearchResult
            {
                Response = new HttpResponseLite
                {
                    Html = await DoGet(urlEndpoint)
                }
            };
            if (result.Response.Html == null)
            {
                result.Succeeded = false;
                return result;
            }

            result.Succeeded = true;
            result.Items = ParseTorrentSearchResult(result.Response.Html);
            return result;
        }

        public abstract List<TorrentSearchItem> ParseTorrentSearchResult(string html);
    }
}