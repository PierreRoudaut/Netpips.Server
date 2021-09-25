using Netpips.Core;
using NUnit.Framework;

namespace Netpips.Tests.Core
{
    [TestFixture]
    public class AppHelperTests
    {

        [Test]
        [Category(TestCategory.LocalDependency)]
        [Category(TestCategory.Filebot)]
        [Category(TestCategory.Transmission)]
        [Category(TestCategory.MediaInfo)]
        [Category(TestCategory.Aria2)]
        public void AssertCliDependenciesTest()
        {
            Assert.DoesNotThrow(AppAsserter.AssertCliDependencies);
        }
    }
}