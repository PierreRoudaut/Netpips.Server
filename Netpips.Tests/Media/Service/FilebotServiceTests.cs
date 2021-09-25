using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Core.Settings;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Service
{
    [TestFixture]
    [Category(TestCategory.LocalDependency)]
    [Category(TestCategory.Filebot)]
    [Category(TestCategory.Integration)]
    public class FilebotServiceTests
    {
        private NetpipsSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = TestHelper.CreateNetpipsAppSettings();
        }

        [Test]
        public void TryRenameTest_Case_Success()
        {

            var path = Path.Combine(settings.DownloadsPath, "The.Big.Bang.Theory.S10E01.FASTSUB.VOSTFR.HDTV.x264-FDS.mkv");
            TestHelper.CreateFile(path);
            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);
            var expectedPath = Path.Combine(
                settings.MediaLibraryPath,
                "TV Shows",
                "The Big Bang Theory",
                "Season 10",
                "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mkv");

            Assert.IsTrue(filebot.TryRename(path, settings.MediaLibraryPath, out var destPath, "Failed to rename using Filebot"));
            Assert.AreEqual(expectedPath, destPath);
        }

        [Test]
        public void TryRenameTest_Case_FileAlreadyExists()
        {
            var path = Path.Combine(settings.DownloadsPath, "The.Big.Bang.Theory.S10E01.FASTSUB.VOSTFR.HDTV.x264-FDS.mp4");
            TestHelper.CreateFile(path);

            var alreadyExistingFilePath = Path.Combine(
                settings.MediaLibraryPath,
                "TV Shows",
                "The Big Bang Theory",
                "Season 10",
                "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4");
            TestHelper.CreateFile(alreadyExistingFilePath);

            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);

            Assert.IsTrue(filebot.TryRename(path, settings.MediaLibraryPath, out var destPath));
            Assert.AreEqual(alreadyExistingFilePath, destPath);
        }


        [Test]
        public void TryRenameTest_Case_Failure()
        {
            var path = Path.Combine(settings.DownloadsPath, Guid.NewGuid().ToString("N") + ".mp4");
            TestHelper.CreateFile(path);
            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);
            Assert.IsFalse(filebot.TryRename(path, settings.MediaLibraryPath, out _));
        }

        [Test]
        public void GetSubtitlesTest_Case_NonStrict()
        {
            var itemDir = Path.Combine(settings.DownloadsPath, TestHelper.Uid());
            Directory.CreateDirectory(itemDir);
            var itemPath = Path.Combine(itemDir, "The Big Bang Theory - S11E17 - The Athenaeum Allocation.mkv");
            TestHelper.CreateFile(itemPath);
            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);
            Assert.IsTrue(filebot.GetSubtitles(itemPath, out var srtPath, nonStrict: true), "filebot -get-subtitles failed");
            Assert.IsTrue(File.Exists(srtPath), ".srt not found");
        }

        [Test]
        public void GetSubtitlesTest_Case_StrictOn_ShouldFail()
        {
            var itemDir = Path.Combine(settings.DownloadsPath, TestHelper.Uid());
            Directory.CreateDirectory(itemDir);
            var itemPath = Path.Combine(itemDir, "The Big Bang Theory - S11E17 - The Athenaeum Allocation.mkv");
            TestHelper.CreateFile(itemPath);
            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);
            var result = filebot.GetSubtitles(itemPath, out var srtPath);
            Assert.IsFalse(result);
        }

        [Test]
        public void GetSubtitlesTest_Case_WithLang_NonStrict()
        {
            var itemDir = Path.Combine(settings.DownloadsPath, TestHelper.Uid());
            Directory.CreateDirectory(itemDir);
            var path = Path.Combine(itemDir, "The Big Bang Theory - S11E17 - The Athenaeum Allocation.mkv");
            TestHelper.CreateFile(path);
            var loggerMock = new Mock<ILogger<IFilebotService>>();
            var filebot = new FilebotService(loggerMock.Object);
            Assert.IsTrue(filebot.GetSubtitles(path, out var srtPath, "eng", nonStrict: true), "filebot -get-subtitles failed");
            Assert.IsTrue(File.Exists(srtPath), ".srt not found");
        }
    }
}