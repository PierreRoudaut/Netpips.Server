using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Search.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

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
        [Category(TestCategory.ThirdParty)]
        public async Task SearchAsyncTest()
        {
            var service = new MagnetDLScrapper(logger.Object);
            var items = await service.SearchAsync("Game of thrones");
            Assert.GreaterOrEqual(items.Count, 10);
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
        public async Task ScrapeTorrentUrlAsyncTest()
        {
            const string scrapeUrl = "https://www.magnetdl.com/file/4824310/star.wars.the.bad.batch.s01e08.720p.webrip.x265-minx-eztv/";
            var service = new MagnetDLScrapper(logger.Object);

            var torrentUrl = await service.ScrapeTorrentUrlAsync(scrapeUrl);
            Assert.AreEqual("magnet:?xt=urn:btih:a15d47a09eaa3eb99211bfd4697387b81845d3ae&dn=Star.Wars.The.Bad.Batch.S01E08.720p.WEBRip.x265-MiNX+%5Beztv%5D&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp://tracker.internetwarriors.net:1337&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969", torrentUrl);
        }
    }
}