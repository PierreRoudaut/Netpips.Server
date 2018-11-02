using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Core.Service;
using Netpips.Download.Authorization;
using Netpips.Download.Controller;
using Netpips.Download.DownloadMethod.PeerToPeer;
using Netpips.Download.Model;
using NUnit.Framework;

namespace Netpips.Tests.Download.Controller
{
    [TestFixture]
    public class TorrentDoneControllerTest
    {
        private Mock<ILogger<TorrentDoneController>> logger;
        private Mock<IControllerHelperService> helper;
        private Mock<IAuthorizationService> authorizationService;
        private Mock<ITorrentDaemonService> torrentService;
        private Mock<IDownloadItemRepository> repository;
        private Mock<IDispatcher> dispatcher;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<TorrentDoneController>>();
            authorizationService = new Mock<IAuthorizationService>();
            torrentService = new Mock<ITorrentDaemonService>();
            helper = new Mock<IControllerHelperService>();
            repository = new Mock<IDownloadItemRepository>();
            dispatcher = new Mock<IDispatcher>();
        }

        [Test]
        public void TorrentDoneTestCaseNotLocallCall()
        {
            helper.Setup(x => x.IsLocalCall(It.IsAny<HttpContext>())).Returns(false);

            var controller = new TorrentDoneController(logger.Object, repository.Object, authorizationService.Object,
                torrentService.Object, dispatcher.Object);

            var response = controller.TorrentDone(helper.Object, "ABCD");
            Assert.AreEqual(403, response.StatusCode);
        }

        [Test]
        public void TorrentDoneTestCaseNotFound()
        {
            helper.Setup(x => x.IsLocalCall(It.IsAny<HttpContext>())).Returns(true);

            repository.Setup(x => x.FindAllUnarchived()).Returns(new List<DownloadItem>());

            var controller = new TorrentDoneController(logger.Object, repository.Object, authorizationService.Object,
                torrentService.Object, dispatcher.Object);

            var response = controller.TorrentDone(helper.Object, "ABCD");

            Assert.AreEqual(404, response.StatusCode);
        }

        [Test]
        public void TorrentDoneTestCaseOk()
        {
            var item = new DownloadItem { Type = DownloadType.DDL, State = DownloadState.Downloading, Hash = "ABCD" };
            helper.Setup(x => x.IsLocalCall(It.IsAny<HttpContext>())).Returns(true);

            repository.Setup(x => x.FindAllUnarchived()).Returns(new List<DownloadItem> {item});

            authorizationService
                .Setup(
                    x => x.AuthorizeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<DownloadItem>(),
                        DownloadItemPolicies.TorrentDonePolicy))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var controller = new TorrentDoneController(logger.Object, repository.Object, authorizationService.Object,
                torrentService.Object, dispatcher.Object);

            var response = controller.TorrentDone(helper.Object, "ABCD");

            Assert.AreEqual(200, response.StatusCode);
        }
    }
}