using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Netpips.Core.Extensions;
using Netpips.Download.Model;
using Netpips.Identity.Authorization;

namespace Netpips.Download.Authorization
{
    public class ItemOwnershipAuthorizationHandler : AuthorizationHandler<ItemOwnershipRequirement, DownloadItem>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ItemOwnershipRequirement requirement,
            DownloadItem item)
        {
            if (context.User.Owns(item) || context.User.GetRole() == Role.SuperAdmin)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class ItemDownloadingAuthorizationHandler : AuthorizationHandler<ItemDownloadingRequirement, DownloadItem>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ItemDownloadingRequirement requirement,
            DownloadItem item)
        {
            if (item.State == DownloadState.Downloading)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class ArchiveItemAuthorizationHandler : AuthorizationHandler<ItemCanceledOrCompletedRequirement, DownloadItem>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ItemCanceledOrCompletedRequirement requirement,
            DownloadItem item)
        {
            if (item.State == DownloadState.Canceled || item.State == DownloadState.Completed)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public enum DownloadItemAuthorizationError
    {
        OperationNotPermitted,
        UserNotAllowed
    }

    public class DownloadItemBaseRequirement : IAuthorizationRequirement
    {
        public int HttpCode { get; }
        public DownloadItemAuthorizationError Error { get; }
        public DownloadItemBaseRequirement(int httpCode, DownloadItemAuthorizationError error)
        {
            HttpCode = httpCode;
            Error = error;
        }
    }

    public class ItemOwnershipRequirement : DownloadItemBaseRequirement
    {
        public ItemOwnershipRequirement() : base(403, DownloadItemAuthorizationError.UserNotAllowed) { }
    }
    public class ItemDownloadingRequirement : DownloadItemBaseRequirement
    {
        public ItemDownloadingRequirement() : base(400, DownloadItemAuthorizationError.OperationNotPermitted) { }
    }

    public class ItemCanceledOrCompletedRequirement : DownloadItemBaseRequirement
    {
        public ItemCanceledOrCompletedRequirement() : base(400, DownloadItemAuthorizationError.OperationNotPermitted) { }
    }

}
