using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Netpips.Core.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StatusController : ControllerBase
    {
        [HttpGet("", Name = "Index")]
        [ProducesResponseType(200)]
        public ObjectResult Index()
        {
            return Ok(new
            {
                Date = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString(),
                BuildAt = System.IO.File.GetLastWriteTime(GetType().Assembly.Location)
            });
        }
    }
}