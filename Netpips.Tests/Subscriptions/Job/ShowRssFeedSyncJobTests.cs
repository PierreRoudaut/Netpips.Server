using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Subscriptions.Job;
using Netpips.Subscriptions.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Subscriptions.Job
{
    [TestFixture]
    public class ShowRssFeedSyncJobTests
    {
        private Mock<ILogger<ShowRssFeedSyncJob>> logger;

        private Mock<IOptions<ShowRssSettings>> options;

        private Mock<IShowRssItemRepository> repository;


        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<ShowRssFeedSyncJob>>();
            this.repository = new Mock<IShowRssItemRepository>();
            this.options = new Mock<IOptions<ShowRssSettings>>();
            this.options
                .SetupGet(x => x.Value)
                .Returns(TestHelper.CreateShowRssSettings);
        }

        [Test]
        public void FetchRssItemsFromFeedTest()
        {
            var service = new ShowRssFeedSyncJob(this.logger.Object, this.options.Object, this.repository.Object);

            var xml = TestHelper.GetRessourceContent("show_rss_polling_feed.xml");

            var items = service.FetchRssItemsFromFeed();
            Assert.Greater(items.Count, 0);
        }

        [Test]
        public void InvokeTest()
        {
            var service = new ShowRssFeedSyncJob(this.logger.Object, this.options.Object, this.repository.Object);
            service.Invoke();
            this.repository.Verify(x => x.SyncFeedItems(It.IsAny<List<ShowRssItem>>()), Times.Once());
        }
    }
}
