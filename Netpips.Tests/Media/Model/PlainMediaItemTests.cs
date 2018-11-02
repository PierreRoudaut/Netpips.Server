using System;
using System.IO;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Media.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Model
{
    [TestFixture]
    public class PlainMediaItemTests
    {
        Mock<IOptions<NetpipsSettings>> settings;

        [SetUp]
        public void Setup()
        {
            settings = new Mock<IOptions<NetpipsSettings>>();
            settings.SetupGet(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings());
        }

        [Test]
        public void CtorTest()
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


            Assert.Throws<InvalidOperationException>(() => new PlainMediaItem(new DirectoryInfo(settings.Object.Value.MediaLibraryPath), settings.Object.Value.MediaLibraryPath));
            var item = new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath);
            Assert.NotNull(item);

        }

        [Test]
        public void RenameTest()
        {
            var fileInfo = new FileInfo(
                Path.Combine(
                    settings.Object.Value.MediaLibraryPath,
                    "Movies",
                    "movie.mkv"));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, "abcd");

            // ok
            var item = new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath);
            Assert.DoesNotThrow(() => item.Rename("newName.mkv"));
            Assert.True(item.FileSystemInfo.Exists);
            Assert.AreEqual("newName.mkv", item.FileSystemInfo.Name);

            // renaming root media folder
            var rootMediaFolder = new PlainMediaItem(new DirectoryInfo(Path.Combine(settings.Object.Value.MediaLibraryPath, "Movies")), settings.Object.Value.MediaLibraryPath);
            Assert.Throws<InvalidOperationException>(() => rootMediaFolder.Rename("New Folder"));

        }

        [Test]
        public void DeleteTest()
        {
            var fileInfo = new FileInfo(
                Path.Combine(
                    settings.Object.Value.MediaLibraryPath,
                    "Movies",
                    "movie.mkv"));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, "abcd");

            // ok
            var item = new PlainMediaItem(fileInfo, settings.Object.Value.MediaLibraryPath);
            Assert.DoesNotThrow(() => item.Delete());
            Assert.False(item.FileSystemInfo.Exists);

            // renaming root media folder
            var rootMediaFolder = new PlainMediaItem(new DirectoryInfo(Path.Combine(settings.Object.Value.MediaLibraryPath, "Movies")), settings.Object.Value.MediaLibraryPath);
            Assert.Throws<InvalidOperationException>(() => rootMediaFolder.Delete());

        }
    }
}