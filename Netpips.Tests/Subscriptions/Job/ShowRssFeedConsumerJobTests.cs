using Microsoft.Extensions.Logging;
using Moq;
using Netpips.Download.Controller;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Identity.Model;
using Netpips.Subscriptions.Job;
using Netpips.Subscriptions.Model;
using NUnit.Framework;

namespace Netpips.Tests.Subscriptions.Job
{
    [TestFixture]
    public class ShowRssFeedConsumerJobTests
    {
        private Mock<ILogger<ShowRssFeedConsumerJob>> logger;


        private Mock<IShowRssItemRepository> showRssItemrepository;
        private Mock<IUserRepository> userRepository;

        private Mock<IDownloadItemService> downloadItemService;


        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<ShowRssFeedConsumerJob>>();
            showRssItemrepository = new Mock<IShowRssItemRepository>();
            userRepository = new Mock<IUserRepository>();
            downloadItemService = new Mock<IDownloadItemService>();
           
        }

        [Test]
        public void InvokeTest_NoItemToConsume()
        {
            showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns((ShowRssItem)null);
            var service = new ShowRssFeedConsumerJob(
                logger.Object,
                showRssItemrepository.Object,
                downloadItemService.Object,
                userRepository.Object);
            service.Invoke();
            DownloadItemActionError error;
            downloadItemService.Verify(x => x.StartDownload(It.IsAny<DownloadItem>(), out error), Times.Never);

        }

        [Test]
        public void InvokeTest_StartFailed()
        {
            userRepository.Setup(c => c.GetDaemonUser()).Returns(new User());
            showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns(new ShowRssItem());
            DownloadItemActionError error;
            downloadItemService.Setup(x => x.StartDownload(It.IsAny<DownloadItem>(), out error)).Returns(false);
            var service = new ShowRssFeedConsumerJob(
                logger.Object,
                showRssItemrepository.Object,
                downloadItemService.Object,
                userRepository.Object);
            service.Invoke();
            showRssItemrepository.Verify(x => x.Update(It.IsAny<ShowRssItem>()), Times.Never);

        }

        [Test]
        public void InvokeTest_Ok()
        {
            userRepository.Setup(c => c.GetDaemonUser()).Returns(new User());
            showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns(new ShowRssItem());
            var item = new DownloadItem();
            DownloadItemActionError error;
            downloadItemService.Setup(x => x.StartDownload(It.IsAny<DownloadItem>(), out error)).Returns(true);
            var service = new ShowRssFeedConsumerJob(
                logger.Object,
                showRssItemrepository.Object,
                downloadItemService.Object,
                userRepository.Object);
            service.Invoke();
            showRssItemrepository.Verify(x => x.Update(It.IsAny<ShowRssItem>()), Times.Once);

        }
    }
}
