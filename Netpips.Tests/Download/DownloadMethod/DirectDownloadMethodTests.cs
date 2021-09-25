using System.Collections.Generic;
using System.IO;
using System.Linq;
using Coravel.Events.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Netpips.Core.Settings;
using Netpips.Download.DownloadMethod.DirectDownload;
using Netpips.Download.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.DownloadMethod
{
    [TestFixture]
    [Category(TestCategory.ThirdParty)]
    public class DirectDownloadMethodTest
    {
        private DirectDownloadMethod downloadMethod;
        private Mock<IOptions<NetpipsSettings>> settingsMock;
        private Mock<IOptions<DirectDownloadSettings>> directDownloadSettings;
        private AutoMocker autoMocker;

        [SetUp]
        public void Setup()
        {
            settingsMock = new Mock<IOptions<NetpipsSettings>>();
            settingsMock.Setup(x => x.Value).Returns(TestHelper.CreateNetpipsAppSettings());
            directDownloadSettings = new Mock<IOptions<DirectDownloadSettings>>();
            directDownloadSettings.Setup(x => x.Value).Returns(TestHelper.CreateDirectDownloadSettings());

            autoMocker = new AutoMocker();
            autoMocker.Use(new Mock<IDispatcher>().Object);
            autoMocker.Use(new Mock<ILogger<DirectDownloadMethod>>().Object);
            autoMocker.Use(settingsMock.Object);
            autoMocker.Use(directDownloadSettings.Object);

            downloadMethod = autoMocker.CreateInstance<DirectDownloadMethod>();
        }

        [Test]
        public void CheckDownloabilityTestCaseValid()
        {

            var item = new DownloadItem
            {
                FileUrl = "http://test-debit.free.fr/1024.rnd"
            };

            var result = downloadMethod.CheckDownloadability(item, new List<string>());
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrEmpty(item.Name));
            Assert.AreEqual(1048576, item.TotalSize);
        }

        [TestCase("http://test-debit.free.fr")]
        [TestCase("http://test-debit.free.fr/lalala")]
        public void CheckDownloabilityTestCaseInvalid(string url)
        {

            var item = new DownloadItem
            {
                FileUrl = url
            };

            var result = downloadMethod.CheckDownloadability(item, new List<string>());
            Assert.IsFalse(result);
        }

        [Test]
        public void Authenticate1FichierTestCaseValid()
        {
            var filehoster = autoMocker.Get<IOptions<DirectDownloadSettings>>().Value.Filehosters.First(f => f.Name == "1fichier");
            List<string> cookies = null;
            Assert.DoesNotThrow(() => cookies = downloadMethod.Authenticate(filehoster));
            Assert.GreaterOrEqual(cookies.Count, 1, "Authentication on " + filehoster.Name + " should have retrieved at least 1 cookie");
        }

        // multiple wrong login attempts locks the IP on 1fichier.com
        //[Test]
        //public void Authenticate1FichierTestCaseInvalid()
        //{
        //    var filehoster = DirectDownloadMethod.FileHosterInfos.First(f => f.Name == "1fichier");
        //    filehoster.CredentialsData["pass"] = "wrongpassword";

        //    Assert.Throws<StartDownloadException>(() => this.downloadMethod.Authenticate(filehoster));
        //}

        [Test]
        public void StartDownloadTest()
        {
            //todo: implement
            var beforeCount = DirectDownloadMethod.DirectDownloadTasks.Count();
        }

        [Test]
        public void CancelTest()
        {
            var item = new DownloadItem { Token = "ABCD" };
            Assert.False(downloadMethod.Cancel(item));

            var task = new Mock<IDirectDownloadTask>();
            DirectDownloadMethod.DirectDownloadTasks[item.Token] = task.Object;
            Assert.True(downloadMethod.Cancel(item));
            task.Verify(t => t.Cancel(), Times.Once);
        }

        [Test]
        public void GetDownloadedSizeTest()
        {
            var item = new DownloadItem { Token = TestHelper.Uid() };

            const string FilenameFormat = "Video {0}.mkv";
            const int DirCount = 3;
            var itemPath = Path.Combine(settingsMock.Object.Value.DownloadsPath, item.Token);
            var tempPath = itemPath;
            for (var i = 1; i <= DirCount; i++)
            {
                Directory.CreateDirectory(tempPath);
                File.WriteAllText(Path.Combine(tempPath, string.Format(FilenameFormat, i)), i.ToString());
            }

            Assert.AreEqual(DirCount, downloadMethod.GetDownloadedSize(item));
        }
    }
}