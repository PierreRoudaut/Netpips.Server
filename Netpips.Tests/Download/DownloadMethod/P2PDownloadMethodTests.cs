using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Netpips.Core.Settings;
using Netpips.Download.DownloadMethod.PeerToPeer;
using Netpips.Download.Exception;
using Netpips.Download.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.DownloadMethod
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class P2PDownloadMethodTests
    {
        private P2PDownloadMethod downloadMethod;
        private Mock<ILogger<P2PDownloadMethod>> logger;
        private Mock<IOptions<NetpipsSettings>> settingsMock;
        private Mock<IAria2CService> ariaService;
        private Mock<ITorrentDaemonService> torrentService;
        private NetpipsSettings settings;


        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<P2PDownloadMethod>>();
            settingsMock = new Mock<IOptions<NetpipsSettings>>();
            ariaService = new Mock<IAria2CService>();
            torrentService = new Mock<ITorrentDaemonService>();

            settings = TestHelper.CreateNetpipsAppSettings();
            settingsMock.Setup(x => x.Value).Returns(settings);
        }

        [TestCase("magnet:?magnetLinkNotFound", P2PDownloadMethod.BrokenMagnetLinkMessage, 1)]
        [TestCase("magnet:?magnetLinkTimeout", P2PDownloadMethod.NoPeersFoundMessage, -1)]
        public void DownloadCaseInvalidMagnet(string magnetLink, string expectedExceptionMessage, int ariaReturnValue)
        {
            var item = new DownloadItem { FileUrl = magnetLink };

            ariaService
                .Setup(x => x.DownloadTorrentFile(It.Is<string>(url => url == magnetLink), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(ariaReturnValue);
            downloadMethod =
                new P2PDownloadMethod(logger.Object, settingsMock.Object, ariaService.Object, torrentService.Object);

            var ex = Assert.Throws<FileNotDownloadableException>(() => downloadMethod.Start(item));
            Assert.AreEqual(expectedExceptionMessage, ex.Message);
        }

        [Test]
        public void DownloadCaseInvalidTorrent()
        {

            downloadMethod =
                new P2PDownloadMethod(logger.Object, settingsMock.Object, ariaService.Object, torrentService.Object);

            var item = new DownloadItem
            {
                FileUrl = "http://invalid-torrent-url.com/file.torrent"
            };

            var ex = Assert.Throws<FileNotDownloadableException>(() => downloadMethod.Start(item));
            Assert.AreEqual(P2PDownloadMethod.TorrentFileNotFoundMessage, ex.Message);
        }

        [Test]
        public void DownloadCaseCorruptedTorrent()
        {
            downloadMethod =
                new P2PDownloadMethod(logger.Object, settingsMock.Object, ariaService.Object, torrentService.Object);

            var corruptedTorrentPath =
                TestHelper.GetRessourceFullPath("The.Big.Bang.Theory.S11E14.HDTV.x264-SVA[rarbg].corrupted.torrent");
            var item = new DownloadItem
            {
                FileUrl = corruptedTorrentPath
            };
            var ex = Assert.Throws<FileNotDownloadableException>(() => downloadMethod.Start(item));
            Assert.AreEqual(P2PDownloadMethod.TorrentFileCorrupted, ex.Message);
        }

        [Test]
        public void DownloadCaseFailedToAddTorrentToDaemon()
        {
            torrentService.Setup(x => x.AddTorrent(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            downloadMethod =
                new P2PDownloadMethod(logger.Object, settingsMock.Object, ariaService.Object, torrentService.Object);

            var torrentPath =
                TestHelper.GetRessourceFullPath("The.Big.Bang.Theory.S11E14.HDTV.x264-SVA[rarbg].torrent");

            var item = new DownloadItem
            {
                FileUrl = torrentPath
            };

            var ex = Assert.Throws<StartDownloadException>(() => downloadMethod.Start(item));
            Assert.AreEqual(P2PDownloadMethod.TorrentDaemonAddFailureMessage, ex.Message);
        }

        [Test]
        public void DownloadCaseValid()
        {
            torrentService.Setup(x => x.AddTorrent(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            downloadMethod =
                new P2PDownloadMethod(logger.Object, settingsMock.Object, ariaService.Object, torrentService.Object);

            var torrentPath =
                TestHelper.GetRessourceFullPath("The.Big.Bang.Theory.S11E14.HDTV.x264-SVA[rarbg].torrent");

            var item = new DownloadItem
            {
                FileUrl = torrentPath
            };

            Assert.DoesNotThrow(() => downloadMethod.Start(item));
            Assert.AreEqual("The.Big.Bang.Theory.S11E14.HDTV.x264-SVA[rarbg]", item.Name);
            Assert.AreEqual(140940255, item.TotalSize);
            Assert.AreEqual("25c8f093021fd9d97087f9444c160d9bb3d70e35", item.Hash);
        }

        [TestCase("https://torrents.yts.rs/torrent/download/227F05638D05C6798B4D86E34429FB7D34474576", true)]
        [TestCase("magnet:?1234", true)]
        [TestCase("http://some-torrent-forum.com/file.torrent", false)]
        [TestCase("https://wikipedia.org", false)]
        [TestCase("https://en.wikipedia.org/404", false)]
        [TestCase("http://test-debit.free.fr/1024.rnd", false)]
        [TestCase("http://invalid-forum.com/1234", false)]
        public void CanHandle(string url, bool expectedResult)
        {
            var autoMocker = new AutoMocker();
            var method = autoMocker.CreateInstance<P2PDownloadMethod>();
            Assert.AreEqual(expectedResult, method.CanHandle(url));
        }
    }
}