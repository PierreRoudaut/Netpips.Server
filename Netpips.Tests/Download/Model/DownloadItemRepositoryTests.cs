using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Core.Model;
using Netpips.Core.Settings;
using Netpips.Download.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.Model
{
    [TestFixture]
    public class DownloadItemRepositoryTest
    {
        private Mock<ILogger<DownloadItemRepository>> logger;
        private Mock<AppDbContext> dbContext;
        private NetpipsSettings settings;

        [SetUp]
        public void Setup()
        {
            settings = TestHelper.CreateNetpipsAppSettings();
            dbContext = new Mock<AppDbContext>();
            logger = new Mock<ILogger<DownloadItemRepository>>();
        }

        [Test]
        public void GetPassedItemsToArchiveTests()
        {
            const int ExpectedItemsCountToArchive = 4;
            var items = new List<DownloadItem>
            {
                new DownloadItem { Archived = false, CanceledAt = DateTime.Now.AddDays(-7), State = DownloadState.Canceled },
                new DownloadItem { Archived = true, CanceledAt = DateTime.Now.AddDays(-6), State = DownloadState.Canceled },
                new DownloadItem { Archived = false, CanceledAt = DateTime.Now.AddDays(-5), State = DownloadState.Canceled },
                new DownloadItem { Archived = false, CanceledAt = DateTime.Now.AddDays(-1), State = DownloadState.Canceled },
                new DownloadItem { Archived = false, CompletedAt = DateTime.Now.AddDays(-7), State = DownloadState.Completed },
                new DownloadItem { Archived = true, CompletedAt = DateTime.Now.AddDays(-6), State = DownloadState.Completed },
                new DownloadItem { Archived = false, CompletedAt = DateTime.Now.AddDays(-5), State = DownloadState.Completed },
                new DownloadItem { Archived = false, CompletedAt = DateTime.Now.AddDays(-1), State = DownloadState.Completed },
            };


            var itemsQueryable = items.AsQueryable();

            var mockSet = new Mock<DbSet<DownloadItem>>();
            mockSet.As<IQueryable<DownloadItem>>().Setup(m => m.Provider).Returns(itemsQueryable.Provider);
            mockSet.As<IQueryable<DownloadItem>>().Setup(m => m.Expression).Returns(itemsQueryable.Expression);
            mockSet.As<IQueryable<DownloadItem>>().Setup(m => m.ElementType).Returns(itemsQueryable.ElementType);
            mockSet.As<IQueryable<DownloadItem>>().Setup(m => m.GetEnumerator()).Returns(itemsQueryable.GetEnumerator());


            dbContext.SetupGet(c => c.DownloadItems).Returns(mockSet.Object);

            var repo = new DownloadItemRepository(logger.Object, dbContext.Object);
            var toArchive = repo.GetPassedItemsToArchive(ExpectedItemsCountToArchive);

            Assert.AreEqual(ExpectedItemsCountToArchive, toArchive.Count);
        }
    }
}