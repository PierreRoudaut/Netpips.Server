using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Search.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Search.Service
{
    [TestFixture]
    public class BitTorrentAmScrapperTests
    {
        public Mock<ILogger<BitTorrentAmScrapper>> Logger;

        [SetUp]
        public void Setup()
        {
            Logger = new Mock<ILogger<BitTorrentAmScrapper>>();
        }

        [Test]
        [Category(TestCategory.ThirdParty)]
        [Category(TestCategory.Integration)]
        [Ignore("bittorrent down")]
        public async Task SearchAsyncTest()
        {
            var service = new BitTorrentAmScrapper(Logger.Object);
            var result = await service.SearchAsync("Game of thrones");
            Assert.GreaterOrEqual(result.Items.Count, 100);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.ThirdParty)]
        [Ignore("bittorrent down")]
        public async Task ScrapeTorrentUrlAsyncTest()
        {
            const string scrapeUrl = "http://bittorrent.am/download-torrent/8068638/100//Armageddon-(1998)-1080p-BrRip-x264-2.00GB-YIFY.html";
            var service = new BitTorrentAmScrapper(Logger.Object);

            var torrentUrl = await service.ScrapeTorrentUrlAsync(scrapeUrl);
            Assert.NotNull(torrentUrl);
            Assert.IsTrue(torrentUrl.StartsWith("magnet:?"));
        }

        [Test]
        public void ParseTorrentSearchResultTest()
        {
            var service = new BitTorrentAmScrapper(Logger.Object);
            var htmlRessourceContent = TestHelper.GetRessourceContent("bittorrentam_search_results.html");

            var items = service.ParseTorrentSearchResult(htmlRessourceContent);
            Assert.AreEqual(100, items.Count);
        }

        [Test]
        public void ParseTorrentDetailResultTest()
        {
            var service = new BitTorrentAmScrapper(Logger.Object);
            var htmlRessourceContent = TestHelper.GetRessourceContent("bittorrentam_torrent_detail_result.html");

            const string ExpectedTorrentUrl = "magnet:?xt=urn:btih:55E5FE0436CD60C2F0CFC210B5E124CC1505F38D&dn=Game.of.Thrones.S07E04.The.Spoils.of.War.360p.WEB-DL&tr=udp://public.popcorn-tracker.org:6969&tr=udp%3A//tracker.leechers-paradise.org%3A6969&tr=udp%3A//zer0day.ch%3A1337&tr=udp%3A//open.demonii.com%3A1337&tr=udp%3A//tracker.coppersurfer.tk%3A6969&tr=udp%3A//exodus.desync.com%3A6969&tr=udp%3A//thetracker.org";
            var url = service.ParseFirstMagnetLinkOrDefault(htmlRessourceContent);
            Assert.AreEqual(ExpectedTorrentUrl, url);
        }
    }
}
