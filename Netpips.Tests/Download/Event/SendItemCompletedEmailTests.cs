using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Netpips.Core.Service;
using Netpips.Core.Settings;
using Netpips.Download.Event;
using Netpips.Download.Model;
using Netpips.Identity.Model;
using Netpips.Media.Model;
using Netpips.Subscriptions.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.Event
{
    [TestFixture]
    public class SendItemCompletedEmailTests
    {
        private AutoMocker autoMocker;

        [SetUp]
        public void Setup()
        {
            autoMocker = new AutoMocker();
            autoMocker
                .GetMock<IOptions<NetpipsSettings>>()
                .SetupGet(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings);
            var item = new DownloadItem
            {
                MovedFiles = new List<MediaItem>
                {
                    new MediaItem { Path = "TV Shows/Armageddon (1998)/Armageddon (1998).mkv", Size = 123456789 }
                },
                Owner = new User { Email = "item-owner@domain.com" }
            };
            autoMocker.GetMock<IDownloadItemRepository>().Setup(x => x.Find(It.IsAny<Guid>())).Returns(item);
        }


        [Test]
        public void HandleAsyncTest_Case_Subscription()
        {
            var list = new List<string> { "subscriber@domain.com" };
            autoMocker
                .GetMock<ITvShowSubscriptionRepository>()
                .Setup(x => x.IsSubscriptionDownload(It.IsAny<DownloadItem>(), out list)).Returns(true);

            var job = autoMocker.CreateInstance<SendItemCompletedEmail>();
            job.HandleAsync(new ItemDownloaded(Guid.Empty));
            autoMocker
                .GetMock<ISmtpService>()
                .Verify(x => x.Send(It.Is<MailMessage>(message =>
                    message.Bcc.Select(t => t.Address).Contains("subscriber@domain.com"))),
                    Times.Once);
        }

        [Test]
        public void HandleAsyncTest_Case_ManualDownload()
        {
            var list = It.IsAny<List<string>>();
            autoMocker
                .GetMock<ITvShowSubscriptionRepository>()
                .Setup(x => x.IsSubscriptionDownload(It.IsAny<DownloadItem>(), out list)).Returns(false);

            var job = autoMocker.CreateInstance<SendItemCompletedEmail>();
            job.HandleAsync(new ItemDownloaded(Guid.Empty));
            autoMocker
                .GetMock<ISmtpService>()
                .Verify(x => x.Send(It.Is<MailMessage>(message =>
                        message.To.Select(t => t.Address).Contains("item-owner@domain.com"))),
                    Times.Once);
        }
    }
}
