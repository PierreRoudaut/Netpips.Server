using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Identity.Authorization;
using Netpips.Identity.Model;
using Netpips.Subscriptions.Controller;
using Netpips.Subscriptions.Model;
using Netpips.Subscriptions.Service;
using NUnit.Framework;

namespace Netpips.Tests.Subscriptions.Controller
{
    [TestFixture]
    public class TvShowSubscriptionControllerTests
    {
        private Mock<ILogger<TvShowController>> logger;

        private Mock<IShowRssGlobalSubscriptionService> showRssService;
        private Mock<IUserRepository> userRepository;
        private Mock<IMemoryCache> memoryCache;
        private Mock<IDownloadItemService> downloadItemService;
        private Mock<IDownloadItemRepository> downloadItemRepository;


        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<TvShowController>>();
            this.showRssService = new Mock<IShowRssGlobalSubscriptionService>();
            this.memoryCache = new Mock<IMemoryCache>();
            this.downloadItemService = new Mock<IDownloadItemService>();
            this.downloadItemRepository = new Mock<IDownloadItemRepository>();
            this.userRepository = new Mock<IUserRepository>();
        }

        #region Unsubscribe

        [Test]
        public void UnsubscribeTest_Case_Show_Not_Subscribed()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions = new List<TvShowSubscription>()
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Unsubscribe(123);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual("Show not subscribed", res.Value);
        }

        [Test]
        public void UnsubscribeTest_Case_ShowRssUnsubscribeFailed()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions =
                    new List<TvShowSubscription>
                        {
                            new TvShowSubscription
                                {
                                    Id = new Guid(),
                                    ShowRssId = 123,
                                    ShowTitle = "ABC"
                                }
                        }
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            this.userRepository.Setup(c => c.IsTvShowSubscribedByOtherUsers(It.IsAny<int>(), It.IsAny<Guid>())).Returns(false);

            var emptySummary = new SubscriptionsSummary();
            this.showRssService.Setup(c => c.Authenticate(out emptySummary)).Returns(new ShowRssAuthenticationContext());
            this.showRssService
                .Setup(c => c.UnsubscribeToShow(It.IsAny<ShowRssAuthenticationContext>(), It.IsAny<int>()))
                .Returns(new ShowRssGlobalSubscriptionService.UnsubscriptionResult { Succeeded = false });

            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Unsubscribe(123);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual("Internal error, failed to unsubscribe to show", res.Value);
        }

        [Test]
        public void UnsubscribeTest_Case_Ok()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions =
                    new List<TvShowSubscription>
                        {
                            new TvShowSubscription
                                {
                                    Id = new Guid(),
                                    ShowRssId = 123,
                                    ShowTitle = "ABC"
                                }
                        }
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            this.userRepository.Setup(c => c.IsTvShowSubscribedByOtherUsers(It.IsAny<int>(), It.IsAny<Guid>())).Returns(false);

            var emptySummary = new SubscriptionsSummary();
            this.showRssService.Setup(c => c.Authenticate(out emptySummary)).Returns(new ShowRssAuthenticationContext());
            this.showRssService
                .Setup(c => c.UnsubscribeToShow(It.IsAny<ShowRssAuthenticationContext>(), It.IsAny<int>()))
                .Returns(new ShowRssGlobalSubscriptionService.UnsubscriptionResult { Succeeded = true });

            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Unsubscribe(123);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual("Unsubscribed", res.Value);
            Assert.AreEqual(0, user.TvShowSubscriptions.Count);
        }
        #endregion

        #region Subscribe

        [Test]
        public void SubscribeTest_Case_Show_Already_Subscribed()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions =
                    new List<TvShowSubscription>
                        {
                            new TvShowSubscription
                                {
                                    Id = new Guid(),
                                    ShowRssId = 123,
                                    ShowTitle = "ABC"
                                }
                        }
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Subscribe(123);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual("Show already subscribed", res.Value);
        }


        [Test]
        public void SubscribeTest_Case_ShowRssSubscribeFailed()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions = new List<TvShowSubscription>()
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            this.userRepository.Setup(c => c.IsTvShowSubscribedByOtherUsers(It.IsAny<int>(), It.IsAny<Guid>())).Returns(false);

            var emptySummary = new SubscriptionsSummary { SubscribedShows = new List<TvShowRss>() };
            this.showRssService.Setup(c => c.Authenticate(out emptySummary)).Returns(new ShowRssAuthenticationContext());
            this.showRssService
                .Setup(c => c.SubscribeToShow(It.IsAny<ShowRssAuthenticationContext>(), It.IsAny<int>()))
                .Returns(new ShowRssGlobalSubscriptionService.SubscriptionResult { Succeeded = false });

            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Subscribe(123);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual("Internal error, failed to subscribe to show", res.Value);
        }

        [Test]
        public void SubscribeTest_Case_Ok()
        {
            var user = new User
            {
                Email = "userwithouttvshowsubscriptions@example.com",
                Role = Role.User,
                FamilyName = "",
                GivenName = "",
                Picture = "",
                Id = Guid.NewGuid(),
                TvShowSubscriptions = new List<TvShowSubscription>()
            };

            this.userRepository.Setup(c => c.FindUser(It.IsAny<Guid>())).Returns(user);
            this.userRepository.Setup(c => c.IsTvShowSubscribedByOtherUsers(It.IsAny<int>(), It.IsAny<Guid>())).Returns(false);

            var emptySummary = new SubscriptionsSummary { SubscribedShows = new List<TvShowRss>() };
            this.showRssService.Setup(c => c.Authenticate(out emptySummary)).Returns(new ShowRssAuthenticationContext());
            this.showRssService
                .Setup(c => c.SubscribeToShow(It.IsAny<ShowRssAuthenticationContext>(), It.IsAny<int>()))
                .Returns(new ShowRssGlobalSubscriptionService.SubscriptionResult { Succeeded = true, Summary = new SubscriptionsSummary { SubscribedShows = new List<TvShowRss> { new TvShowRss { ShowRssId = 123 } } } });

            var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
            };

            var res = controller.Subscribe(123);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual("Subscribed", res.Value);
            Assert.AreEqual(1, user.TvShowSubscriptions.Count);
            Assert.AreEqual(123, user.TvShowSubscriptions.First().ShowRssId);
        }

        #endregion

        //[Test]
        //public void GetTvMazeIdTest()
        //{
        //    var user = new User
        //    {
        //        Email = "userwithouttvshowsubscriptions@example.com",
        //        Role = Role.User,
        //        FamilyName = "",
        //        GivenName = "",
        //        Picture = "",
        //        Id = Guid.NewGuid(),
        //        TvShowSubscriptions = new List<TvShowSubscription>()
        //    };

        //    var controller = new TvShowController(this.logger.Object, this.showRssService.Object, this.userRepository.Object, this.memoryCache.Object)
        //    {
        //        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user.MapToClaimPrincipal() } }
        //    };

        //    var res = controller.GetTvMazeId(150);
        //    Assert.AreEqual(63, res.Value);

        //}

    }
}
