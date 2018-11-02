using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Media.Model;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Model
{
    [TestFixture]
    public class MediaItemRepositoryTests
    {
        private Mock<ILogger<MediaLibraryService>> logger;

        private Mock<IOptions<NetpipsSettings>> settings;

        [SetUp]
        public void Setup()
        {
            settings = new Mock<IOptions<NetpipsSettings>>();
            settings.SetupGet(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings());
            logger = new Mock<ILogger<MediaLibraryService>>();
        }

        [Test]
        public void FindAllTest()
        {
            var fileInfo = new FileInfo(Path.Combine(settings.Object.Value.MediaLibraryPath, "TV Shows", "The Big Bang Theory", "Season 01", "The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4"));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, "abcd");

            var expectedItems = new List<PlainMediaItem>();
            expectedItems.AddRange(
                NetpipsSettings.MediaFolders.Select(
                    f => new PlainMediaItem(new DirectoryInfo(Path.Combine(settings.Object.Value.MediaLibraryPath,f)), settings.Object.Value.MediaLibraryPath)));
            expectedItems.Add(
                new PlainMediaItem(
                    new DirectoryInfo(Path.Combine(settings.Object.Value.MediaLibraryPath, "TV Shows", "The Big Bang Theory")),
                    settings.Object.Value.MediaLibraryPath));
            expectedItems.Add(
                new PlainMediaItem(
                    new DirectoryInfo(Path.Combine(settings.Object.Value.MediaLibraryPath, "TV Shows", "The Big Bang Theory", "Season 01")),
                    settings.Object.Value.MediaLibraryPath));
            expectedItems.Add(new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath));

            var repo = new MediaItemRepository(logger.Object, settings.Object);

            var items = repo.FindAll();
            Assert.That(items, Is.EquivalentTo(expectedItems).Using(new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath)));
        }

        [Test]
        public void FindTest()
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

            var repo = new MediaItemRepository(logger.Object, settings.Object);

            var file = repo.Find(
                "TV Shows/The Big Bang Theory/Season 01/The Big Bang Theory - S10E01 - The Conjugal Conjecture.mp4");
            Assert.AreEqual(file.Parent, "TV Shows/The Big Bang Theory/Season 01");
            Assert.AreEqual(4, file.Size);


            var dir = repo.Find("TV Shows/The Big Bang Theory/Season 01");
            Assert.AreEqual(dir.Parent, "TV Shows/The Big Bang Theory");
            Assert.Null(dir.Size);

            Assert.Null(repo.Find("TV Shows/inexistant_file.mkc"));
            Assert.Null(repo.Find(""));
            Assert.Null(repo.Find(".."));
        }
    }
}