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
            this.logger = new Mock<ILogger<ShowRssFeedConsumerJob>>();
            this.showRssItemrepository = new Mock<IShowRssItemRepository>();
            this.userRepository = new Mock<IUserRepository>();
            this.downloadItemService = new Mock<IDownloadItemService>();
           
        }

        [Test]
        public void InvokeTest_NoItemToConsume()
        {
            this.showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns((ShowRssItem)null);
            var service = new ShowRssFeedConsumerJob(
                this.logger.Object,
                this.showRssItemrepository.Object,
                this.downloadItemService.Object,
                this.userRepository.Object);
            service.Invoke();
            DownloadItemActionError error;
            this.downloadItemService.Verify(x => x.StartDownload(It.IsAny<DownloadItem>(), out error), Times.Never);

        }

        [Test]
        public void InvokeTest_StartFailed()
        {
            this.userRepository.Setup(c => c.GetDaemonUser()).Returns(new User());
            this.showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns(new ShowRssItem());
            DownloadItemActionError error;
            this.downloadItemService.Setup(x => x.StartDownload(It.IsAny<DownloadItem>(), out error)).Returns(false);
            var service = new ShowRssFeedConsumerJob(
                this.logger.Object,
                this.showRssItemrepository.Object,
                this.downloadItemService.Object,
                this.userRepository.Object);
            service.Invoke();
            this.showRssItemrepository.Verify(x => x.Update(It.IsAny<ShowRssItem>()), Times.Never);

        }

        [Test]
        public void InvokeTest_Ok()
        {
            this.userRepository.Setup(c => c.GetDaemonUser()).Returns(new User());
            this.showRssItemrepository.Setup(x => x.FindFirstQueuedItem()).Returns(new ShowRssItem());
            var item = new DownloadItem();
            DownloadItemActionError error;
            this.downloadItemService.Setup(x => x.StartDownload(It.IsAny<DownloadItem>(), out error)).Returns(true);
            var service = new ShowRssFeedConsumerJob(
                this.logger.Object,
                this.showRssItemrepository.Object,
                this.downloadItemService.Object,
                this.userRepository.Object);
            service.Invoke();
            this.showRssItemrepository.Verify(x => x.Update(It.IsAny<ShowRssItem>()), Times.Once);

        }
    }
}
