using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Netpips.Core.Service
{
    public class ControllerHelperService : IControllerHelperService
    {
        [ExcludeFromCodeCoverage]
        public bool IsLocalCall(HttpContext context) => Equals(
                context.Request.HttpContext.Connection.RemoteIpAddress,
                context.Features.Get<IHttpConnectionFeature>().LocalIpAddress);
    }
}
