using System;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netpips.Download.Model;

namespace Netpips.Core.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StatusController : ControllerBase
    {
        private readonly IDownloadItemRepository repository;

        public StatusController(IDownloadItemRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("", Name = "Status")]
        [ProducesResponseType(200)]
        public ObjectResult Status()
        {
            var now = DateTime.UtcNow;
            var lastBuildAt = System.IO.File.GetLastWriteTimeUtc(GetType().Assembly.Location);
            var elapsed = now.AddMilliseconds(-now.Subtract(lastBuildAt).TotalMilliseconds);

            var canRedeploy = !repository.HasPendingDownloads();

            return Ok(new
            {
                Date = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                Build = new
                {
                    Timestamp = lastBuildAt,
                    Elapsed = elapsed.Humanize()
                },
                CanRedeploy = canRedeploy
            });
        }
    }
}