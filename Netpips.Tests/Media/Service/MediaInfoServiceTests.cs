using Netpips.Media.MediaInfo;
using Netpips.Media.Service;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Media.Service
{
    [TestFixture]
    public class MediaInfoServiceTests
    {
        [Test]
        [Category(TestCategory.MediaInfo)]
        [Category(TestCategory.LocalDependency)]
        public void GetDurationTest()
        {
            var ressourceFullPath = TestHelper.GetRessourceFullPath("1718ms-video-file.mp4");
            var service = new MediaInfoService();
            var succeeded = service.TryGetDuration(ressourceFullPath, out var duration);
            Assert.IsTrue(succeeded);
            Assert.AreEqual(1718, duration.TotalMilliseconds);
        }
    }
}