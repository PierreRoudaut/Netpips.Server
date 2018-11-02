
using System;
using Microsoft.AspNetCore.Authorization;

namespace Netpips.Core.Authorization
{
    public abstract class BaseRequirement : IAuthorizationRequirement
    {
        public int HttpCode { get; }
        public Enum Error { get; }

        protected BaseRequirement(int httpCode, Enum error)
        {
            this.HttpCode = httpCode;
            this.Error = error;
        }
    }
}
