using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Extensions;
using Netpips.Core.Settings;
using Netpips.Subscriptions.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Subscriptions.Service
{
    [TestFixture]
    public class ShowRssGlobalSubscriptionServiceTests
    {
        private Mock<ILogger<ShowRssGlobalSubscriptionService>> logger;

        private Mock<IOptions<ShowRssSettings>> options;
        private ShowRssSettings settings;

        [SetUp]
        public void Setup()
        {
            settings = TestHelper.CreateShowRssSettings();
            logger = new Mock<ILogger<ShowRssGlobalSubscriptionService>>();
            options = new Mock<IOptions<ShowRssSettings>>();
            options
                .SetupGet(x => x.Value)
                .Returns(settings);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.ThirdParty)]
        public void AuthenticateTest()
        {
            var service = new ShowRssGlobalSubscriptionService(logger.Object, options.Object);
            service.Authenticate(out var result);
            Assert.Greater(result.AvailableShows.Count, 900);
        }

        [Test]
        public void ParseSubscriptionsResultTest()
        {
            var service = new ShowRssGlobalSubscriptionService(logger.Object, options.Object);
            var html = TestHelper.GetRessourceContent("showRSS.html");

            var result = service.ParseSubscriptionsResult(html);
            Assert.AreEqual(4, result.SubscribedShows.Count);
            Assert.AreEqual(988, result.AvailableShows.Count);
            Assert.IsTrue(result.SubscribedShows.Any(f => f.ShowTitle.Equals("game of thrones", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void ParseCsrfTokenTest()
        {
            var service = new ShowRssGlobalSubscriptionService(logger.Object, options.Object);
            var html = TestHelper.GetRessourceContent("showRSS.html");
            var token = service.ParseCsrfToken(html);

            Assert.AreEqual("Xe8AdkOS5vo11JjQSXVRbZ48e3I7bPGw6daYrEQy", token);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.ThirdParty)]
        public void SubscribeUnsubscribeShowTest()
        {
            var service = new ShowRssGlobalSubscriptionService(logger.Object, options.Object);
            var context = service.Authenticate(out var initialResult);

            var show = initialResult.AvailableShows.Random();
            Assert.NotNull(show);

            // subscribe
            var afterSubscriptionResult = service.SubscribeToShow(context, show.ShowRssId);
            Assert.IsTrue(afterSubscriptionResult.Succeeded);

            // unsubscribe
            var afterUnsubscriptionResult = service.UnsubscribeToShow(context, show.ShowRssId);
            Assert.IsTrue(afterUnsubscriptionResult.Succeeded);
        }
    }
}
