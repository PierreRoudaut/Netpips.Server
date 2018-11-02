using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coravel.Events.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpips.Core.Settings;
using Netpips.Download.Controller;
using Netpips.Download.DownloadMethod;
using Netpips.Download.Exception;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Identity.Authorization;
using Netpips.Identity.Model;
using Netpips.Identity.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.Service
{
    [TestFixture]
    public class DownloadItemServiceTest
    {
        private Mock<IDispatcher> dispatcher;
        private Mock<ILogger<DownloadItemService>> logger;
        private Mock<IOptions<NetpipsSettings>> options;
        private Mock<IDownloadItemRepository> repository;
        private Mock<IDownloadMethod> downloadMethod;

        public static User ItemOwner = new User { Email = "owner@example.com" };
        public static User NotAnOwner = new User { Email = "notanowner@example.com" };

        private readonly HttpContext OwnerHttpContext =
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] {
                                      new Claim(JwtRegisteredClaimNames.Email, ItemOwner.Email),
                                      new Claim(JwtRegisteredClaimNames.FamilyName, ""),
                                      new Claim(JwtRegisteredClaimNames.GivenName, "") ,
                                      new Claim(AppClaims.Picture, ""),
                                      new Claim(ClaimsIdentity.DefaultRoleClaimType, Role.User.ToString()),
                            })
                    )
            };

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<DownloadItemService>>();
            dispatcher = new Mock<IDispatcher>();
            options = new Mock<IOptions<NetpipsSettings>>();
            options.Setup(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings());
            repository = new Mock<IDownloadItemRepository>();
            downloadMethod = new Mock<IDownloadMethod>();
        }

        [Test]
        public void StartDownloadTestCaseUrlNotHandled()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<string>())).Returns(false);
            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });

            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);

            Assert.IsFalse(service.StartDownload(new DownloadItem { FileUrl = "not-handled-url.com/123" }, out var err));
            Assert.AreEqual(err, DownloadItemActionError.UrlNotHandled);
        }


        [Test]
        public void StartDownloadTestCaseFileNotDownaloadable()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<string>())).Returns(true);
            downloadMethod
                .Setup(m => m.Start(It.IsAny<DownloadItem>()))
                .Throws(new FileNotDownloadableException("File not downloadable"));

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });
            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);
            var result = service.StartDownload(new DownloadItem { FileUrl = "handled-url.com/notAFile" }, out var err);

            Assert.IsFalse(result);
            Assert.AreEqual(err, DownloadItemActionError.DownloadabilityFailure);
        }

        [Test]
        public void StartDownloadTestCaseStartDownloadFailure()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<string>())).Returns(true);
            downloadMethod
                .Setup(m => m.Start(It.IsAny<DownloadItem>()))
                .Throws(new StartDownloadException("Failed to start download"));

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });
            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);
            var result = service.StartDownload(new DownloadItem { FileUrl = "handled-url.com" }, out var err);

            Assert.IsFalse(result);
            Assert.AreEqual(err, DownloadItemActionError.StartDownloadFailure);
        }


        [Test]
        public void StartDownloadTestCaseOk()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<string>())).Returns(true);

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });
            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);
            var result = service.StartDownload(new DownloadItem { FileUrl = "handled-url.com" }, out _);
            Assert.True(result);
            downloadMethod.Verify(x => x.Start(It.IsAny<DownloadItem>()), Times.Once);
        }

        [Test]
        public void CancelDownload()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<DownloadType>())).Returns(true);

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });
            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);

            var item = new DownloadItem();
            service.CancelDownload(item);

            downloadMethod.Verify(x => x.Cancel(item), Times.Once);
        }

        [Test]
        public void ArchiveDownload()
        {
            downloadMethod.Setup(x => x.CanHandle(It.IsAny<DownloadType>())).Returns(true);

            var services = new ServiceCollection();
            services.AddScoped(typeof(IDownloadMethod), delegate { return downloadMethod.Object; });
            var serviceProvider = services.BuildServiceProvider();

            var service = new DownloadItemService(logger.Object, serviceProvider, options.Object, dispatcher.Object, repository.Object);

            var item = new DownloadItem { Token = "ABCD" };
            service.ArchiveDownload(item);

            downloadMethod.Verify(x => x.Archive(item), Times.Once);
        }
    }
}