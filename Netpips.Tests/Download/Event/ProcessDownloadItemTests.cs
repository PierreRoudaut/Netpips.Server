using System;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Download.Event;
using Netpips.Download.Model;
using Netpips.Media.Service;
using NUnit.Framework;

namespace Netpips.Tests.Download.Event
{
    [TestFixture]
    public class ProcessDownloadItemTests
    {
        private Mock<ILogger<ProcessDownloadItem>> logger;
        private Mock<IDownloadItemRepository> repository;
        private Mock<IMediaLibraryMover> mediaLibraryMover;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<ProcessDownloadItem>>();
            repository = new Mock<IDownloadItemRepository>();
            mediaLibraryMover = new Mock<IMediaLibraryMover>();
        }

        [Test]
        public void HandleAsyncTest()
        {
            var item = new DownloadItem { };
            repository.Setup(x => x.Find(It.IsAny<Guid>())).Returns(item);

            var job = new ProcessDownloadItem(logger.Object, repository.Object, mediaLibraryMover.Object);
            job.HandleAsync(new ItemDownloaded(new Guid()));
            repository.Verify(x => x.Update(It.IsAny<DownloadItem>()), Times.Exactly(2));
        }
    }
}
