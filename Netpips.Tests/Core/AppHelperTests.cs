using Netpips.Core;
using NUnit.Framework;

namespace Netpips.Tests.Core
{
    [TestFixture]
    public class AppHelperTests
    {

        [Test]
        // [Category(TestCategory.LocalDependency)]
        // [Category(TestCategory.Filebot)]
        //[Category(TestCategory.Transmission)]
        [Category(TestCategory.Aria2)]
        [Category(TestCategory.MediaInfo)]
        public void AssertCliDependenciesTest()
        {
            Assert.DoesNotThrow(AppAsserter.AssertCliDependencies);
        }
    }
}