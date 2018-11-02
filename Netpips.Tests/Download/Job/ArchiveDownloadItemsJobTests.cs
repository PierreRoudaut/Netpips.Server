using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Download.Job;
using Netpips.Download.Model;
using Netpips.Download.Service;
using NUnit.Framework;

namespace Netpips.Tests.Download.Job
{
    [TestFixture]
    public class ArchiveDownloadItemsJobTests
    {
        private  Mock<ILogger<ArchiveDownloadItemsJob>> logger;

        private Mock<IDownloadItemRepository> repository;

        private Mock<IDownloadItemService> service;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<ArchiveDownloadItemsJob>>();
            repository = new Mock<IDownloadItemRepository>();
            service = new Mock<IDownloadItemService>();
        }

        [Test]
        public void Invoke()
        {
            var items = new List<DownloadItem>
            {
                new DownloadItem { Archived = false, CanceledAt = DateTime.Now.AddDays(-7), State = DownloadState.Canceled },
            };
            repository.Setup(x => x.GetPassedItemsToArchive(It.IsAny<int>())).Returns(items);

            var job = new ArchiveDownloadItemsJob(logger.Object, repository.Object, service.Object);
            job.Invoke();
            service.Verify(x => x.ArchiveDownload(It.IsAny<DownloadItem>()), Times.Once);

        }
    }
}