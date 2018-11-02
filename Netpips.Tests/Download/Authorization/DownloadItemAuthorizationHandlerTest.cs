using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Netpips.Download.Authorization;
using Netpips.Download.Model;
using Netpips.Tests.Core;
using NUnit.Framework;

namespace Netpips.Tests.Download.Authorization
{
    [TestFixture]
    public class ItemOwnershipAuthorizationHandlerTest
    {
        [Test]
        public void HandleRequirementTestSameOwner()
        {
            var item = new DownloadItem { OwnerId = TestHelper.ItemOwner.Id };
            var requirement = new ItemOwnershipRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.ItemOwner.MapToClaimPrincipal(),
                item
            );

            var handler = new ItemOwnershipAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test]
        public void HandleRequirementTestIsSuperAdmin()
        {
            var item = new DownloadItem { Owner = TestHelper.ItemOwner };
            var requirement = new ItemOwnershipRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.SuperAdmin.MapToClaimPrincipal(),
                item
            );

            var handler = new ItemOwnershipAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test]
        public void HandleRequirementTestIsAdmin()
        {
            var item = new DownloadItem { Owner = TestHelper.ItemOwner };
            var requirement = new ItemOwnershipRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.Admin.MapToClaimPrincipal(),
                item
            );

            var handler = new ItemOwnershipAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test]
        public void HandleRequirementTestDifferentOwner()
        {
            var item = new DownloadItem { Owner = TestHelper.ItemOwner };
            var requirement = new ItemOwnershipRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.NotAnItemOwner.MapToClaimPrincipal(),
                item
            );

            var handler = new ItemOwnershipAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.False(context.HasSucceeded);
            Assert.AreEqual(403, ((DownloadItemBaseRequirement)context.Requirements.First()).HttpCode);
            Assert.AreEqual(DownloadItemAuthorizationError.UserNotAllowed, ((DownloadItemBaseRequirement)context.Requirements.First()).Error);
        }
    }

    [TestFixture]
    public class ItemDownloadingAuthorizationHandlerTest
    {
        [Test]
        public void HandleRequirementTest()
        {
            var item = new DownloadItem { Owner = TestHelper.ItemOwner, State = DownloadState.Downloading};
            var requirement = new ItemDownloadingRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.ItemOwner.MapToClaimPrincipal(),
                item
            );

            var handler = new ItemDownloadingAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.True(context.HasSucceeded);
        }

    }

    [TestFixture]
    public class ArchiveItemAuthorizationHandlerTest
    {
        [TestCase(DownloadState.Completed, true)]
        [TestCase(DownloadState.Canceled, true)]
        [TestCase(DownloadState.Downloading, false)]
        [TestCase(DownloadState.Processing, false)]
        public void HandleRequirementTest(DownloadState state, bool res)
        {
            var item = new DownloadItem { Owner = TestHelper.ItemOwner, State  = state};
            var requirement = new ItemCanceledOrCompletedRequirement();

            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                TestHelper.ItemOwner.MapToClaimPrincipal(),
                item
            );

            var handler = new ArchiveItemAuthorizationHandler();
            handler.HandleAsync(context).Wait();
            Assert.AreEqual(res, context.HasSucceeded);
        }

    }
}
