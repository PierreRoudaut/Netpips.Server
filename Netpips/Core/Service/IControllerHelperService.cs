using Microsoft.AspNetCore.Http;

namespace Netpips.Core.Service
{
    public interface IControllerHelperService
    {
        bool IsLocalCall(HttpContext context);
    }
}