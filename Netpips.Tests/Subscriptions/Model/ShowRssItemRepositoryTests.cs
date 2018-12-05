using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Core.Model;
using Netpips.Download.Model;
using Netpips.Subscriptions.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Netpips.Tests.Subscriptions.Model
{
    [TestFixture]
    public class ShowRssItemRepositoryTests
    {
        private Mock<ILogger<ShowRssItemRepository>> logger;
        private Mock<AppDbContext> dbContext;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<ShowRssItemRepository>>();
            this.dbContext = new Mock<AppDbContext>();
        }


        [Test]
        public void SyncFeedItemsTest()
        {
            var showRssItems = new List<ShowRssItem>
                                   {
                                       new ShowRssItem { Guid = "abcd" },
                                       new ShowRssItem { Guid = "ijkl" }
                                   };
            var showRssItemsQueryable = showRssItems.AsQueryable();

            var mockSet = new Mock<DbSet<ShowRssItem>>();
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.Provider).Returns(showRssItemsQueryable.Provider);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.Expression).Returns(showRssItemsQueryable.Expression);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.ElementType).Returns(showRssItemsQueryable.ElementType);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.GetEnumerator()).Returns(showRssItemsQueryable.GetEnumerator());

            var newItems = new List<ShowRssItem>
                               {
                                   new ShowRssItem { Guid = "abcd" },
                                   new ShowRssItem { Guid = "efgh" },
                                   new ShowRssItem { Guid = "ijkl" },
                                   new ShowRssItem { Guid = "mnop"}
                               };

            this.dbContext.SetupGet(c => c.ShowRssItems).Returns(mockSet.Object);

            var repo = new ShowRssItemRepository(this.logger.Object, this.dbContext.Object);
            repo.SyncFeedItems(newItems);
            this.dbContext.Verify(c => c.SaveChanges(), Times.Once);

        }

        [TestCase(6, 5)]
        [TestCase(3, 3)]
        [TestCase(2, 2)]
        public void FindCompletedItemsTest(int timeWindow, int expectedNbItems)
        {
            var showRssItems = new List<ShowRssItem>
            {
                new ShowRssItem { Guid = "not-started" },
                new ShowRssItem {
                    Guid = "downloading",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Downloading
                    }
                },
                new ShowRssItem {
                    Guid = "completed-4days-ago",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Completed,
                        CompletedAt = DateTime.Now.AddDays(-4)
                    }
                },
                new ShowRssItem {
                    Guid = "completed-3days-ago",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Completed,
                        CompletedAt = DateTime.Now.AddDays(-3)
                    }
                },
                new ShowRssItem {
                    Guid = "completed-2days-ago",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Completed,
                        CompletedAt = DateTime.Now.AddDays(-2)
                    }
                },
                new ShowRssItem {
                    Guid = "completed-1day-ago",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Completed,
                        CompletedAt = DateTime.Now.AddDays(-1)
                    }
                },
                new ShowRssItem {
                    Guid = "completed-now",
                    DownloadItem = new DownloadItem {
                        State = DownloadState.Completed,
                        CompletedAt = DateTime.Now
                    }
                }
            };
            var showRssItemsQueryable = showRssItems.AsQueryable();

            var mockSet = new Mock<DbSet<ShowRssItem>>();
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.Provider).Returns(showRssItemsQueryable.Provider);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.Expression).Returns(showRssItemsQueryable.Expression);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.ElementType).Returns(showRssItemsQueryable.ElementType);
            mockSet.As<IQueryable<ShowRssItem>>().Setup(m => m.GetEnumerator()).Returns(showRssItemsQueryable.GetEnumerator());

            this.dbContext.SetupGet(c => c.ShowRssItems).Returns(mockSet.Object);

            var repo = new ShowRssItemRepository(this.logger.Object, this.dbContext.Object);
            var items = repo.FindRecentCompletedItems(timeWindow);
            Assert.AreEqual(expectedNbItems, items.Count);
        }

    }
}