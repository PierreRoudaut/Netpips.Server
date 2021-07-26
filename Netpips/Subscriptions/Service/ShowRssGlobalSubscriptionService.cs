using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;
using Netpips.Subscriptions.Model;

namespace Netpips.Subscriptions.Service
{
    public class ShowRssAuthenticationContext
    {
        public CookieContainer CookieContainer { get; set; }
        public string CsrfToken { get; set; }
    }

    public class SubscriptionsSummary
    {
        public List<TvShowRss> SubscribedShows { get; set; }
        public List<TvShowRss> AvailableShows { get; set; }
    }


    public class ShowRssGlobalSubscriptionService : IShowRssGlobalSubscriptionService
    {
        private readonly ILogger<ShowRssGlobalSubscriptionService> logger;
        private readonly ShowRssSettings settings;
        public ShowRssGlobalSubscriptionService(ILogger<ShowRssGlobalSubscriptionService> logger, IOptions<ShowRssSettings> options)
        {
            this.logger = logger;
            settings = options.Value;
        }

        private List<TvShowRss> ParseSubscribedShows(HtmlDocument htmlDocument)
        {
            var subscribedShows = htmlDocument
                .DocumentNode
                .Descendants("ul").First(u => u.HasClass("user-selections"))
                .Descendants("li")
                .Select(li => new TvShowRss
                {
                    ShowTitle = HttpUtility.HtmlDecode(li.Descendants("a").Last().Attributes["title"].Value.Trim()),
                    ShowRssId = li.Descendants("span").First().Attributes["data-id"].Value.ToInt()
                })
                .ToList();
            return subscribedShows;
        }

        private List<TvShowRss> ParseAvailableShows(HtmlDocument htmlDocument)
        {
            var availableShows = htmlDocument
                .GetElementbyId("showselector")
                .Descendants("option")
                .Where(n => !string.IsNullOrEmpty(n.Attributes["value"]?.Value))
                .Select(o => new TvShowRss
                {
                    ShowTitle = HttpUtility.HtmlDecode(o.InnerText.Trim()),
                    ShowRssId = o.Attributes["value"].Value.ToInt()
                })
                .ToList();
            return availableShows;
        }

        public SubscriptionsSummary ParseSubscriptionsResult(string htmlContent)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            return new SubscriptionsSummary
            {
                SubscribedShows = ParseSubscribedShows(htmlDocument),
                AvailableShows = ParseAvailableShows(htmlDocument)
            };
        }

        public abstract class BaseSubscribeActionResult
        {
            public SubscriptionsSummary Summary { get; set; }
            public bool Succeeded { get; set; }
        }

        public class SubscriptionResult : BaseSubscribeActionResult
        {
        }

        public class UnsubscriptionResult : BaseSubscribeActionResult
        {
        }


        public SubscriptionResult SubscribeToShow(ShowRssAuthenticationContext context, int showId)
        {
            logger.LogInformation("Subscribing to show " + showId);
            using (var handler = new HttpClientHandler() { UseCookies = true, AllowAutoRedirect = true, CookieContainer = context.CookieContainer })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://showrss.info/show/add")
                {
                    Content = new FormUrlEncodedContent(
                        new Dictionary<string, string>
                            {
                                { "_token", context.CsrfToken },
                                { "show", showId.ToString() }
                            })
                };
                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("SubscribeToShow failed " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return new SubscriptionResult { Succeeded = false };
                }

                var html = response.Content.ReadAsStringAsync().Result;

                var summary = ParseSubscriptionsResult(html);

                return new SubscriptionResult
                {
                    Summary = summary,
                    Succeeded = summary.SubscribedShows.Any(s => s.ShowRssId == showId)
                };
            }
        }

        public UnsubscriptionResult UnsubscribeToShow(ShowRssAuthenticationContext context, int showId)
        {
            logger.LogInformation("Unsubscribing to show " + showId);
            using (var handler = new HttpClientHandler() { UseCookies = true, AllowAutoRedirect = true, CookieContainer = context.CookieContainer })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://showrss.info/show/delete")
                {
                    Content = new FormUrlEncodedContent(
                        new Dictionary<string, string>
                            {
                                { "_token", context.CsrfToken },
                                { "show", showId.ToString() }
                            })
                };
                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("SubscribeToShow failed " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return new UnsubscriptionResult { Succeeded = false };
                }
                var html = response.Content.ReadAsStringAsync().Result;
                var summary = ParseSubscriptionsResult(html);

                return new UnsubscriptionResult
                {
                    Summary = summary,
                    Succeeded = summary.AvailableShows.Any(s => s.ShowRssId == showId)
                };
            }
        }

        public string ParseCsrfToken(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var token = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.Attributes["name"].Value == "_token")?.Attributes["value"].Value;
            return token;
        }

        public ShowRssAuthenticationContext Authenticate(out SubscriptionsSummary subscriptions)
        {
            logger.LogInformation("ShowRssService: Authenticate");
            subscriptions = null;
            using (var handler = new HttpClientHandler() { UseCookies = true, AllowAutoRedirect = true })
            using (var client = new HttpClient(handler))
            {
                // initial request to get csrf token
                var request = new HttpRequestMessage(HttpMethod.Get, "https://showrss.info/login");
                client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
                var response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
                var token = ParseCsrfToken(response.Content.ReadAsStringAsync().Result);

                // authentication request
                var credentialsData = new Dictionary<string, string> { { "username", settings.Username }, { "password", settings.Password } };
                credentialsData["_token"] = token;
                var loginRequest = new HttpRequestMessage(HttpMethod.Post, "https://showrss.info/login")
                {
                    Content = new FormUrlEncodedContent(credentialsData)
                };
                response = client.SendAsync(loginRequest).Result;
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Authenticate failed " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return null;
                }
                var html = response.Content.ReadAsStringAsync().Result;

                subscriptions = ParseSubscriptionsResult(html);

                return new ShowRssAuthenticationContext
                {
                    CookieContainer = handler.CookieContainer,
                    CsrfToken = token
                };
            }
        }
    }
}
