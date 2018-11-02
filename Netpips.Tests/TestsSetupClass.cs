using System.Globalization;
using NUnit.Framework;

namespace Netpips.Tests
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            CultureInfo.DefaultThreadCurrentCulture = Program.EnUsCulture;
            CultureInfo.CurrentCulture = Program.EnUsCulture;
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
        }
    }
}
