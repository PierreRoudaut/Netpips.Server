
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netpips.Core.Extensions;
using Netpips.Search.Model;
using Netpips.Search.Service;

namespace Netpips.Search.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class TorrentSearchController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<TorrentSearchController> logger;

        private readonly IMemoryCache memoryCache;

        private readonly IServiceProvider serviceProvider;

        private static readonly TimeSpan SearchTimeout = TimeSpan.FromMilliseconds(3500);

        public TorrentSearchController(ILogger<TorrentSearchController> logger, IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet("", Name = "SearchAsyncParallel")]
        [ProducesResponseType(typeof(IList<TorrentSearchItem>), 200)]
        public async Task<ObjectResult> SearchAsyncParallel([FromQuery] string q)
        {
            q = q.Replace("'", "");
            var cacheSearchKey = $"[torrent-search][{q}]";
            this.logger.LogInformation(cacheSearchKey);
            var items = new List<TorrentSearchItem>();
            if (!this.memoryCache.TryGetValue(cacheSearchKey, out items))
            {
                this.logger.LogInformation("No items retrieved from cache");
                var searchScrappers = this.serviceProvider.GetServices<ITorrentSearchScrapper>().ToList();
                this.logger.LogInformation($"Using [{searchScrappers.Count}] scrappers [{string.Join(", ", searchScrappers.Select(x => x.GetType().Name))}] with timeout {SearchTimeout.TotalMilliseconds} ms");

                var tasks = searchScrappers.Select(s => s.SearchAsync(q));
                var agregatedResults = await tasks.WhenAll(SearchTimeout);
                this.logger.LogInformation($"[{agregatedResults.Length}] scrappers completed within timeout");
                items = agregatedResults.SelectMany(c => c).OrderByDescending(r => r.Seeders).Take(20).ToList();

                this.logger.LogInformation($"[{agregatedResults.Length}] scrappers completed within timeout [{items.Count}] total");
                var breakdown = items.GroupBy(c => new Uri(c.ScrapeUrl).Host);
                this.logger.LogInformation(string.Join(", ", breakdown.Select(g => $"[{g.Key} => {g.Count()}]")));
                this.memoryCache.Set(cacheSearchKey, items, TimeSpan.FromMinutes(5));
            }
            else
            {
                this.logger.LogInformation($"[{items.Count}] items retrieved from cache");
            }
            return Ok(items);
        }

        [HttpPost("scrapeTorrentUrl", Name = "ScrapeTorrentUrl")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ObjectResult> ScrapeTorrentUrlAsync([FromBody] string scrapeUrl)
        {
            var scrapeUri = new Uri(scrapeUrl);
            var torrentDetailScrapper = this.serviceProvider.GetServices<ITorrentDetailScrapper>().FirstOrDefault(s => s.CanScrape(scrapeUri));
            if (torrentDetailScrapper == null)
            {
                return this.BadRequest("Not handled url");
            }

            var torrentUrl = await torrentDetailScrapper.ScrapeTorrentUrlAsync(scrapeUrl);
            if (torrentUrl == null)
            {
                return this.NotFound("Torrent link not found. Please try another link");
            }
            return this.Ok(torrentUrl);
        }
    }
}
