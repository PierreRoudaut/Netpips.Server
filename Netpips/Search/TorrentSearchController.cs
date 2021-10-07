
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
            logger.LogInformation(cacheSearchKey);
            var items = new List<TorrentSearchItem>();
            if (!memoryCache.TryGetValue(cacheSearchKey, out items))
            {
                logger.LogInformation("No items retrieved from cache");
                var searchScrappers = serviceProvider.GetServices<ITorrentSearchScrapper>().ToList();
                logger.LogInformation($"Using [{searchScrappers.Count}] scrappers [{string.Join(", ", searchScrappers.Select(x => x.GetType().Name))}] with timeout {SearchTimeout.TotalMilliseconds} ms");

                var tasks = searchScrappers.Select(s => s.SearchAsync(q));
                var agregatedResults = await tasks.WhenAll(SearchTimeout);
                logger.LogInformation($"[{agregatedResults.Length}] scrappers completed within timeout");
                items = agregatedResults.Where(x => x.Succeeded).SelectMany(c => c.Items).OrderByDescending(r => r.Seeders).Take(20).ToList();

                logger.LogInformation($"[{agregatedResults.Length}] scrappers completed within timeout [{items.Count}] total");
                var breakdown = items.GroupBy(c => new Uri(c.ScrapeUrl).Host);
                logger.LogInformation(string.Join(", ", breakdown.Select(g => $"[{g.Key} => {g.Count()}]")));
                memoryCache.Set(cacheSearchKey, items, TimeSpan.FromMinutes(5));
            }
            else
            {
                logger.LogInformation($"[{items.Count}] items retrieved from cache");
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
            var torrentDetailScrapper = serviceProvider.GetServices<ITorrentDetailScrapper>().FirstOrDefault(s => s.CanScrape(scrapeUri));
            if (torrentDetailScrapper == null)
            {
                return BadRequest("Not handled url");
            }

            var torrentUrl = await torrentDetailScrapper.ScrapeTorrentUrlAsync(scrapeUrl);
            if (torrentUrl == null)
            {
                return NotFound("Torrent link not found. Please try another link");
            }
            return Ok(torrentUrl);
        }
    }
}
