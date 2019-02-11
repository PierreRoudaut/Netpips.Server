using System;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Netpips.Core.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StatusController : ControllerBase
    {
        [HttpGet("", Name = "Status")]
        [ProducesResponseType(200)]
        public ObjectResult Status()
        {
            var now = DateTime.Now;
            var lastBuildAt = System.IO.File.GetLastWriteTime(GetType().Assembly.Location);
            var elapsed = now.AddMilliseconds(-now.Subtract(lastBuildAt).TotalMilliseconds);

            return Ok(new
            {
                Date = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                Build = new
                {
                    Timestamp = lastBuildAt,
                    Elapsed = elapsed.Humanize()
                }
            });
        }
    }
}