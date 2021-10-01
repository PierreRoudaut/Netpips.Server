using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Search.Service;
using Netpips.Tests.Core;
using NUnit.Framework;
using Python.Runtime;

namespace Netpips.Tests.Search.Service
{
    [TestFixture]
    public class MagnetDLScrapperTests
    {
        private Mock<ILogger<MagnetDLScrapper>> logger;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<MagnetDLScrapper>>();
        }

        [Test]
        [Category(TestCategory.Failing)]
        [Category(TestCategory.ThirdParty)]
        public async Task SearchAsyncTest()
        {
            // python -c "import cfscrape; scraper = cfscrape.create_scraper(); r = scraper.get('https://www.magnetdl.com/t/the-bad-batch-s01e09/'); print(r.status_code); print(r.text)"
            var service = new MagnetDLScrapper(logger.Object);
            var result = await service.SearchAsync("Game of thrones");
            Assert.IsTrue(result.Succeeded, $"Search failed{Environment.NewLine}{result.ToStringOfProperties()}");
            Assert.GreaterOrEqual(result.Items.Count, 10);
        }

        [Test]
        public void ParseTorrentSearchItemsTest()
        {
            
            var service = new MagnetDLScrapper(logger.Object);
            var htmlRessourceContent = TestHelper.GetRessourceContent("magnetdl_search_results.html");

            var items = service.ParseTorrentSearchResult(htmlRessourceContent);
            Assert.AreEqual(items.Count, 29);
            var lastItem = items[28];
            Assert.AreEqual("Star.Wars.The.Bad.Batch.S01E08.720p.WEBRip.x265-MiNX [eztv]", lastItem.Title);
            Assert.AreEqual(
                "https://www.magnetdl.com/file/4824310/star.wars.the.bad.batch.s01e08.720p.webrip.x265-minx-eztv/",
                lastItem.ScrapeUrl);
            Assert.AreEqual(
                "magnet:?xt=urn:btih:a15d47a09eaa3eb99211bfd4697387b81845d3ae&dn=Star.Wars.The.Bad.Batch.S01E08.720p.WEBRip.x265-MiNX+%5Beztv%5D&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp://tracker.internetwarriors.net:1337&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969",
                lastItem.TorrentUrl);
            Assert.AreEqual(12, lastItem.Leechers);
            Assert.AreEqual(54, lastItem.Seeders);
            Assert.AreEqual(95221187, lastItem.Size);
        }
        
        [Test]
        [Category(TestCategory.ThirdParty)]
        [Category(TestCategory.Failing)]
        public async Task ScrapeTorrentUrlAsyncTest()
        {
            const string scrapeUrl = "https://www.magnetdl.com/file/4824310/star.wars.the.bad.batch.s01e08.720p.webrip.x265-minx-eztv/";
            var service = new MagnetDLScrapper(logger.Object);

            var torrentUrl = await service.ScrapeTorrentUrlAsync(scrapeUrl);
            Assert.IsTrue(torrentUrl.StartsWith("magnet:?xt=urn:btih:a15d47a09eaa3eb99211bfd4697387b81845d3ae&dn=Star.Wars.The.Bad.Batch.S01E08.720p.WEBRip.x265-MiNX+%5Beztv%5D"));
            // Assert.AreEqual("magnet:?xt=urn:btih:a15d47a09eaa3eb99211bfd4697387b81845d3ae&dn=Star.Wars.The.Bad.Batch.S01E08.720p.WEBRip.x265-MiNX+%5Beztv%5D&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp://tracker.internetwarriors.net:1337&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969", torrentUrl);
        }
    }
}