using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Download.Authorization;
using Netpips.Download.Controller;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.Controller
{
    [TestFixture]
    public class DownloadItemControllerTest
    {
        private Mock<IDownloadItemService> service;
        private Mock<ILogger<DownloadItemController>> logger;
        private Mock<IDownloadItemRepository> repository;
        private Mock<IAuthorizationService> authorizationService;


        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<DownloadItemController>>();
            service = new Mock<IDownloadItemService>();
            repository = new Mock<IDownloadItemRepository>();
            authorizationService = new Mock<IAuthorizationService>();
        }

        [Test]
        public void ListTest()
        {
            repository
                .Setup(x => x.FindAllUnarchived())
                .Returns(() => new List<DownloadItem>
                {
                    new DownloadItem(),
                    new DownloadItem()
                });

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var items = controller.List().Value as IEnumerable<DownloadItem>;

            Assert.AreEqual(2, items.Count());
        }

        [TestCase("existingToken", 200)]
        [TestCase("nonExistingToken", 404)]
        public void GetTest(string token, int statusCode)
        {
            repository
                .Setup(x => x.Find(It.IsAny<string>()))
                .Returns(statusCode < 300 ? new DownloadItem() : null);

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Get(token);

            Assert.AreEqual(statusCode, response.StatusCode);
        }

        [Test]
        public void StartTestTestDuplicateDownload()
        {
            var fileUrl = "ABCD";

            repository.Setup(x => x.FindAllUnarchived())
                .Returns(new List<DownloadItem> { new DownloadItem { FileUrl = "ABCD" } });

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Start(fileUrl);

            Assert.AreEqual(400, response.StatusCode);
        }

        [TestCase(500, DownloadItemActionError.StartDownloadFailure)]
        [TestCase(404, DownloadItemActionError.DownloadabilityFailure)]
        [TestCase(400, DownloadItemActionError.UrlNotHandled)]
        public void StartTestTestFailure(int httpCode, DownloadItemActionError error)
        {
            service.Setup(x => x.StartDownload(It.IsAny<DownloadItem>(), out error)).Returns(false);

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object,
                repository.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext {User = TestHelper.ItemOwner.MapToClaimPrincipal()}
                }
            };
            var response = controller.Start(It.IsAny<string>());
            Assert.AreEqual(httpCode, response.StatusCode);
        }

        [Test]
        public void StartTestOk()
        {
            var item = new DownloadItem();
            DownloadItemActionError error;
            service.Setup(x => x.StartDownload(item, out error)).Returns(true);
            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = TestHelper.ItemOwner.MapToClaimPrincipal() }
                }
            };
            var response = controller.Start(It.IsAny<string>());
            Assert.AreEqual(201, response.StatusCode);
        }


        [Test]
        public void CancelTestCaseNotFound()
        {
            repository.Setup(x => x.Find(It.IsAny<string>())).Returns((DownloadItem)null);

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Cancel(It.IsAny<string>());

            Assert.AreEqual(404, response.StatusCode);
        }


        [Test]
        public void CancelTestCaseUnauthorized()
        {
            var item = new DownloadItem();
            service
                .Setup(x => x.CancelDownload(item));

            repository.Setup(x => x.Find(It.IsAny<string>())).Returns(item);

            authorizationService
                .Setup(
                    x => x.AuthorizeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<DownloadItem>(),
                        DownloadItemPolicies.CancelPolicy))
                .Returns(Task.FromResult(AuthorizationResult.Failed()));

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);

            Assert.Throws<InvalidOperationException>(() => controller.Cancel(It.IsAny<string>()));
        }

        [Test]
        public void CancelTestTestOk()
        {
            var item = new DownloadItem();
            service.Setup(x => x.CancelDownload(item));
            repository.Setup(x => x.Find(It.IsAny<string>())).Returns(item);

            authorizationService
                .Setup(
                    x => x.AuthorizeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<DownloadItem>(),
                        DownloadItemPolicies.CancelPolicy))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Cancel(It.IsAny<string>());

            service.Verify(x => x.CancelDownload(item), Times.Once);

            Assert.AreEqual(200, response.StatusCode);
        }


        [Test]
        public void ArchiveTestCaseNotFound()
        {
            repository.Setup(x => x.Find(It.IsAny<string>())).Returns((DownloadItem)null);

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Archive(It.IsAny<string>());

            Assert.AreEqual(404, response.StatusCode);
        }


        [Test]
        public void ArchiveTestCaseUnauthorized()
        {
            var item = new DownloadItem();
            service
                .Setup(x => x.ArchiveDownload(item));

            repository.Setup(x => x.Find(It.IsAny<string>())).Returns(item);

            authorizationService
                .Setup(
                    x => x.AuthorizeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<DownloadItem>(),
                        DownloadItemPolicies.ArchivePolicy))
                .Returns(Task.FromResult(AuthorizationResult.Failed()));

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);

            Assert.Throws<InvalidOperationException>(() => controller.Archive(It.IsAny<string>()));
        }


        [Test]
        public void ArchiveTestCaseOk()
        {
            var item = new DownloadItem();
            service
                .Setup(x => x.ArchiveDownload(item));
            repository.Setup(x => x.Find(It.IsAny<string>())).Returns(item);

            authorizationService
                .Setup(
                    x => x.AuthorizeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<DownloadItem>(),
                        DownloadItemPolicies.ArchivePolicy))
                .Returns(Task.FromResult(AuthorizationResult.Success()));

            var controller = new DownloadItemController(logger.Object, service.Object, authorizationService.Object, repository.Object);
            var response = controller.Archive(It.IsAny<string>());

            service.Verify(x => x.ArchiveDownload(item), Times.Once);

            Assert.AreEqual(200, response.StatusCode);
        }

    }
}