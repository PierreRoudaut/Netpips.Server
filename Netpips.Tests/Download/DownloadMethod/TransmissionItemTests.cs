using System.Linq;
using Netpips.Download.DownloadMethod.PeerToPeer;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.DownloadMethod
{
    [TestFixture]
    public class TransmissionItemTests
    {
        [Test]
        public void ParsePropertiesCaseValid()
        {
            var summaryStats = TestHelper.GetRessourceContent("transmission-remote-summary-stats.valid.txt");
            var fullStats = TestHelper.GetRessourceContent("transmission-remote-full-stats.valid.txt");
            var transmissionItem = new TransmissionItem(summaryStats, fullStats);

            Assert.AreEqual(4, transmissionItem.Id);
            Assert.AreEqual("Game.of.Thrones.S07E07.The.Dragon.and.the.Wolf.1080p.AMZN.WEBRip.DDP5.1.x264-GoT[rarbg]", transmissionItem.Name);
            Assert.AreEqual(5476083302, transmissionItem.TotalSize);
            Assert.AreEqual(4781506, transmissionItem.DownloadedSize);
            Assert.IsTrue(transmissionItem.Error.Contains("Permission denied"));

            Assert.AreEqual(2, transmissionItem.Files.Count());
        }

        [Test]
        public void ParsePropertiesCaseInvalid()
        {
            var summaryStats = TestHelper.GetRessourceContent("transmission-remote-summary-stats.invalid.txt");
            var transmissionItem = new TransmissionItem(summaryStats, null);

            Assert.AreEqual(5, transmissionItem.Id);
            Assert.AreEqual("Shameless+US+S07E10+720p+HDTV+X264+DIMENSION", transmissionItem.Name);
            Assert.AreEqual(0, transmissionItem.TotalSize);
            Assert.AreEqual(0, transmissionItem.DownloadedSize);
        }
    }
}
