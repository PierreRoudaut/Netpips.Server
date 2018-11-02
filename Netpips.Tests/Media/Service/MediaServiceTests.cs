using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Media.Model;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Service
{
    [TestFixture]
    public class MediaServiceTests
    {
        private Mock<ILogger<MediaLibraryService>> logger;

        private Mock<IOptions<NetpipsSettings>> settings;

        private Mock<IMediaLibraryMover> mover;
        private Mock<IFilebotService> filebot;

        [SetUp]
        public void Setup()
        {
            settings = new Mock<IOptions<NetpipsSettings>>();
            mover = new Mock<IMediaLibraryMover>();
            settings.SetupGet(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings());
            logger = new Mock<ILogger<MediaLibraryService>>();
            filebot = new Mock<IFilebotService>();
        }

        [Test]
        public void AutoRenameTest()
        {
            var fileInfo = new FileInfo(
                Path.Combine(
                    settings.Object.Value.MediaLibraryPath,
                    "TV Shows",
                    "The Big Bang Theory",
                    "Season 01",
                    "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4"));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, "abcd");


            // directory
            var service = new MediaLibraryService(logger.Object, settings.Object, mover.Object, filebot.Object);
            var item = new PlainMediaItem(fileInfo.Directory, settings.Object.Value.MediaLibraryPath);
            Assert.Null(service.AutoRename(item));

            mover.Setup(x => x.MoveVideoFile(fileInfo.FullName)).Returns(new List<FileSystemInfo>());
            service = new MediaLibraryService(logger.Object, settings.Object, mover.Object, filebot.Object);
            service.AutoRename(new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath));
            mover.Verify(x => x.MoveVideoFile(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetSubtitlesTest()
        {
            var fileInfo = new FileInfo(
                Path.Combine(
                    settings.Object.Value.MediaLibraryPath,
                    "TV Shows",
                    "The Big Bang Theory",
                    "Season 01",
                    "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4"));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, "abcd");


            // directory
            var service = new MediaLibraryService(logger.Object, settings.Object, mover.Object, filebot.Object);
            var item = new PlainMediaItem(fileInfo.Directory, settings.Object.Value.MediaLibraryPath);
            Assert.Null(service.GetSubtitles(item, "eng"));

            // no subtitles found
            string _;
            filebot.Setup(x => x.GetSubtitles(It.IsAny<string>(), out _, It.IsAny<string>(), It.IsAny<bool>())).Returns(false);
            service = new MediaLibraryService(logger.Object, settings.Object, mover.Object, filebot.Object);
            item = new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath);
            Assert.Null(service.GetSubtitles(item, "eng"));


            // subtitles found
            var srtPath = Path.Combine(
                Path.GetDirectoryName(fileInfo.FullName),
                Path.GetFileNameWithoutExtension(fileInfo.Name) + ".eng.srt");
            File.WriteAllText(srtPath, "aaa");
            filebot.Setup(x => x.GetSubtitles(It.IsAny<string>(), out srtPath, It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
            service = new MediaLibraryService(logger.Object, settings.Object, mover.Object, filebot.Object);
            item = new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath);
            var srtItem = service.GetSubtitles(item, "eng");
            Assert.AreEqual(3, srtItem.Size);
            Assert.AreEqual("TV Shows/The Big Bang Theory/Season 01", srtItem.Parent);
        }
    }
}