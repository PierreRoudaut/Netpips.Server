using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Service
{
    [TestFixture]
    public class ArchiveServiceTests
    {
        private Mock<IOptions<NetpipsSettings>> optionsMock;

        private Mock<ILogger<ArchiveExtractorService>> loggerMock;

        private NetpipsSettings settings;

        [SetUp]
        public void Setup()
        {
            optionsMock = new Mock<IOptions<NetpipsSettings>>();
            loggerMock = new Mock<ILogger<ArchiveExtractorService>>();

            settings = TestHelper.CreateNetpipsAppSettings();
            optionsMock.Setup(x => x.Value).Returns(settings);


        }

        [TestCase("archiveRAR4.rar")]
        [TestCase("archiveRAR5.rar")]
        public void HandleRarFileCaseSinglePart(string archiveFilename)
        {
            var service = new ArchiveExtractorService(loggerMock.Object, optionsMock.Object);

            // archive.rar content:
            // -------------------
            // /music.mp3
            // /pinguin.gif
            // /subfolder/file.mkv

            var archivePath = Path.Combine(settings.DownloadsPath, archiveFilename);
            File.Copy(TestHelper.GetRessourceFullPath(archiveFilename), archivePath);

            Assert.IsTrue(service.HandleRarFile(archivePath, out var extractedDirectoryPath));

            CollectionAssert.AreEquivalent(
                new List<string> { "music.mp3", "pinguin.gif", "file.mkv" }, 
                Directory.GetFiles(extractedDirectoryPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName));
        }

        [Test]
        public void HandleRarFileCaseMultiPartIncomplete()
        {
            var parts = Enumerable.Range(1, 5).RandomSubsequence(4).Select(n => string.Format("music.part{0}.rar", n)).ToList();
            var partToHandle = parts.Random();
            var existingParts = parts.Where(p => p != partToHandle);
            var part = "";

            foreach (var existingPart in existingParts)
            {
                part = Path.Combine(settings.MediaLibraryPath, "Others", "music", existingPart);
                FilesystemHelper.SafeCopy(TestHelper.GetRessourceFullPath(existingPart), part);
            }

            // Create and copy new .part.rar file to be handled
            part = Path.Combine(settings.DownloadsPath, "_5678", partToHandle);
            FilesystemHelper.SafeCopy(TestHelper.GetRessourceFullPath(partToHandle), part);

            var service = new ArchiveExtractorService(loggerMock.Object, optionsMock.Object);
            Assert.False(service.HandleRarFile(part, out var extractedDirectoryPath));

            // Ensure that new part.rar file has been moved
            Assert.AreEqual(4, Directory.GetFiles(extractedDirectoryPath, "*", SearchOption.AllDirectories).Length);
        }


        [Test]
        public void HandleRarFileCaseMultiParComplete()
        {
            var allParts = Enumerable.Range(1, 5).Select(n => string.Format("music.part{0}.rar", n)).ToList();
            var partToHandle = allParts.Random();
            var part = "";

            foreach (var existingPart in allParts)
            {
                part = Path.Combine(settings.MediaLibraryPath, "Others", "music", existingPart);
                FilesystemHelper.SafeCopy(TestHelper.GetRessourceFullPath(existingPart), part);
            }

            // Create and copy new .part.rar file to be handled
            part = Path.Combine(settings.DownloadsPath, "_5678", partToHandle);
            FilesystemHelper.SafeCopy(TestHelper.GetRessourceFullPath(partToHandle), part);

            var service = new ArchiveExtractorService(loggerMock.Object, optionsMock.Object);
            Assert.True(service.HandleRarFile(part, out var destDir));

            // assert .part0n.rar files are deleted
            Assert.AreEqual(1, Directory.GetFiles(destDir, "*", SearchOption.AllDirectories).Length);

            // assert music.mp3 has been extracted and is the right length
            var file = new  FileInfo(Path.Combine(settings.MediaLibraryPath, "Others", "music", "music.mp3"));
            Assert.IsTrue(file.Exists, "File should be extracted");
            Assert.AreEqual(4487785, file.Length);
        }

    }
}
