using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Service
{
    [TestFixture]
    [Category(TestCategory.Filesystem)]
    public class MediaLibraryMoverTests
    {
        private Mock<ILogger<MediaLibraryMover>> loggerMock;
        private Mock<IOptions<NetpipsSettings>> settingsMock;
        private Mock<IFilebotService> filebotMock;
        private Mock<IMediaInfoService> mediaInfoMock;
        private Mock<IArchiveExtractorService> archiveMock;
        private NetpipsSettings settings;

        [SetUp]
        public void Setup()
        {
            this.settings = TestHelper.CreateNetpipsAppSettings();

            this.loggerMock = new Mock<ILogger<MediaLibraryMover>>();
            this.settingsMock = new Mock<IOptions<NetpipsSettings>>();
            this.settingsMock.Setup(x => x.Value).Returns(this.settings);
            this.filebotMock = new Mock<IFilebotService>();
            this.mediaInfoMock = new Mock<IMediaInfoService>();
            this.archiveMock = new Mock<IArchiveExtractorService>();
        }

        [Test]
        public void MoveMusicItemTest()
        {

            var mover = new MediaLibraryMover(this.settingsMock.Object, this.loggerMock.Object, this.filebotMock.Object, this.mediaInfoMock.Object, this.archiveMock.Object);

            var musicFilename = "Cosmic Gate - Be Your Sound.mp3";
            var musicSrcPath = Path.Combine(this.settings.DownloadsPath, TestHelper.Uid(), musicFilename);

            TestHelper.CreateFile(musicSrcPath);
            var musicDestPath = mover.MoveMusicFile(musicSrcPath);
            Assert.IsTrue(File.Exists(musicDestPath.FullName), "music test file was not moved");
            Assert.AreEqual(Path.Combine(this.settings.MediaLibraryPath, "Music", musicFilename), musicDestPath.FullName);

        }

        [Test]
        public void MoveVideoItemTestCaseRenameOk()
        {

            var videoSrcPath = Path.Combine(this.settings.DownloadsPath, TestHelper.Uid(), "the.big.bang.theory.s10.e01.mp4");
            TestHelper.CreateFile(videoSrcPath);

            var videoDestPath = Path.Combine(this.settings.MediaLibraryPath, "TV Shows", "The Big Bang Theory", "Season 10", "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4");

            this.filebotMock.Setup(x => x.TryRename(It.IsAny<string>(), this.settings.MediaLibraryPath, out videoDestPath, null, "test")).Returns(true);
            var mover = new MediaLibraryMover(this.settingsMock.Object, this.loggerMock.Object, this.filebotMock.Object, this.mediaInfoMock.Object, this.archiveMock.Object);
            var fsItems = mover.MoveVideoFile(videoSrcPath);

            Assert.AreEqual(3, fsItems.Count, "Tbbt folder and season folder should be created");
            Assert.IsTrue(File.Exists(videoDestPath), "Dest video file was not created");
            Assert.IsFalse(File.Exists(videoSrcPath), "Src video is still present");

        }

        [TestCase(0, "Others")]
        [TestCase(1, "TV Shows")]
        [TestCase(40, "TV Shows")]
        [TestCase(105, "Movies")]
        [TestCase(135, "Movies")]
        public void MoveVideoItemTestCaseRenameKo(int minutes, string fallbackDir)
        {

            const string Filename = "Unknown Video.mkv";

            var videoSrcPath = Path.Combine(this.settings.DownloadsPath, TestHelper.Uid(), Filename);
            TestHelper.CreateFile(videoSrcPath);

            var duration = TimeSpan.FromMinutes(minutes);
            this.mediaInfoMock.Setup(x => x.TryGetDuration(It.IsAny<string>(), out duration)).Returns(true);

            string _;
            this.filebotMock.Setup(x => x.TryRename(It.IsAny<string>(), It.IsAny<string>(), out _, null, It.IsAny<string>())).Returns(false);

            var mover = new MediaLibraryMover(this.settingsMock.Object, this.loggerMock.Object, this.filebotMock.Object, this.mediaInfoMock.Object, this.archiveMock.Object);
            var movedFsItems = mover.MoveVideoFile(videoSrcPath);

            var videoDestPath = Path.Combine(this.settings.MediaLibraryPath, fallbackDir, Filename);

            Assert.IsTrue(File.Exists(videoDestPath), "Dest video file was not created");
            Assert.IsFalse(File.Exists(videoSrcPath), "Src video is still present");
            Assert.AreEqual(1, movedFsItems.Count);
            Assert.AreEqual(videoDestPath, movedFsItems.First().FullName);
        }

        [Test]
        public void MoveMatchingSubtitlesOfTest()
        {

            var srcFilename = "the.big.bang.theory.s10.e01.mp4";
            //var handledSubs = new List<string> { ".srt", ".en.srt", ".fr.srt", ".eng.srt", ".fra.srt" };
            var handledSubs = new Dictionary<string, string>
            {
                { ".srt", ".srt" },
                { ".en.srt", ".en.srt" },
                { ".eng.srt", ".en.srt" },
                { ".fra.srt", ".fr.srt" },
                { ".fr.srt", ".fr.srt" },
            };
            var videoSrcPath = Path.Combine(this.settings.DownloadsPath, TestHelper.Uid(), srcFilename);

            TestHelper.CreateFile(videoSrcPath);
            handledSubs.ToList().ForEach(subExt =>
            {
                TestHelper.CreateFile(videoSrcPath.GetPathWithoutExtension() + subExt.Key);
            });

            var videoDestPath = Path.Combine(this.settings.MediaLibraryPath, "TV Shows", "The Big Bang Theory", "Season 10", "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4");
            TestHelper.CreateFile(videoDestPath);

            var downloadCompletedHandler = new MediaLibraryMover(this.settingsMock.Object, this.loggerMock.Object, this.filebotMock.Object, this.mediaInfoMock.Object, this.archiveMock.Object);
            var movedSubs = downloadCompletedHandler.MoveMatchingSubtitlesOf(videoSrcPath, videoDestPath);

            handledSubs.ToList().ForEach(subExt =>
            {
                Assert.IsTrue(movedSubs.Any(sub => sub.FullName == videoDestPath.GetPathWithoutExtension() + subExt.Value));
                Assert.IsFalse(File.Exists(videoSrcPath.GetPathWithoutExtension() + subExt));
            });
        }

        [Test]
        public void ProcessItemTest()
        {
            var filenameFormat = "Video {0}.mkv";
            int dirCount = 3;
            var itemPath = Path.Combine(this.settings.DownloadsPath, TestHelper.Uid());
            var tempPath = itemPath;
            for (int i = 1; i <= dirCount; i++)
            {
                Directory.CreateDirectory(tempPath);
                TestHelper.CreateFile(Path.Combine(tempPath, string.Format(filenameFormat, i)));
                tempPath = Path.Combine(tempPath, TestHelper.Uid());
            }
            var _ = "";
            var duration = TimeSpan.FromMinutes(0);
            this.filebotMock.Setup(x => x.TryRename(It.IsAny<string>(), It.IsAny<string>(), out _, null, It.IsAny<string>())).Returns(false);
            this.filebotMock.Setup(x => x.GetSubtitles(It.IsAny<string>(), out _, It.IsAny<string>(), It.IsAny<bool>())).Returns(false);
            this.mediaInfoMock.Setup(x => x.TryGetDuration(It.IsAny<string>(), out duration)).Returns(true);

            var mediaLibraryMover = new MediaLibraryMover(this.settingsMock.Object, this.loggerMock.Object, this.filebotMock.Object, this.mediaInfoMock.Object, this.archiveMock.Object);

            mediaLibraryMover.ProcessDir(itemPath);
            for (int i = 1; i <= dirCount; i++)
            {
                Assert.IsTrue(File.Exists(Path.Combine(this.settings.MediaLibraryPath, "Others", string.Format(filenameFormat, i))));
            }
        }
    }
}