using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Core.Model;
using Netpips.Subscriptions.Model;
using NUnit.Framework;

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

    }
}