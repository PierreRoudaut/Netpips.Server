using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Netpips.Core.Settings;
using Netpips.Download.Model;
using Netpips.Media.Model;
using Netpips.Media.Service;
using Netpips.Subscriptions.Job;
using Netpips.Subscriptions.Model;
using Netpips.Tests.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Netpips.Media.Filebot;

namespace Netpips.Tests.Subscriptions.Job
{
    [TestFixture]
    public class GetMissingSubtitlesTests
    {
        private Mock<IOptions<NetpipsSettings>> options;
        private NetpipsSettings settings;
        private Mock<IFilebotService> filebot;

        private Mock<IShowRssItemRepository> repository;

        private AutoMocker autoMocker;

        [SetUp]
        public void Setup()
        {
            autoMocker = new AutoMocker();
            filebot = new Mock<IFilebotService>();
            options = new Mock<IOptions<NetpipsSettings>>();
            settings = TestHelper.CreateNetpipsAppSettings();
            options.SetupGet(x => x.Value).Returns(settings);
            autoMocker.Use(options.Object);
            autoMocker.Use(filebot.Object);
            repository = new Mock<IShowRssItemRepository>();
        }

        [Test]
        public void InvokeTest()
        {
            // finish implementation

            var movedItem = new DownloadItem
            {
                MovedFiles = new List<MediaItem>
                {
                    new MediaItem
                    {
                        Path = "TV Shows/Suits/Season 01/Suits S01E01 Episode Name.mkv"
                    }
                }
            };

            var path = Path.Combine(options.Object.Value.MediaLibraryPath,
                "TV Shows", "Game Of Thrones", "Season 01", "Game Of Thrones S01E01 Episode Name.mkv");
            TestHelper.CreateFile(path);
            var pmi = new PlainMediaItem(new FileInfo(path), options.Object.Value.MediaLibraryPath);

            var missingSubItem = new DownloadItem
            {
                MovedFiles = new List<MediaItem>
                {
                    new MediaItem
                    {
                        Path = pmi.Path
                    }
                }
            };

            var items = new List<DownloadItem> { movedItem, missingSubItem };
            repository.Setup(x => x.FindRecentCompletedItems(It.IsAny<int>())).Returns(items);
            autoMocker.Use(repository.Object);

            var job = autoMocker.CreateInstance<GetMissingSubtitlesJob>();
            job.Invoke();
            var outSrtPath = It.IsAny<string>();
            filebot.Verify(x => x.GetSubtitles(It.IsAny<string>(), out outSrtPath, It.IsAny<string>(), false), Times.Exactly(2));
        }
    }
}
