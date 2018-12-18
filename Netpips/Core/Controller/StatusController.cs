using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Netpips.Core.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StatusController : ControllerBase
    {
        [HttpGet("", Name = "Hello")]
        [ProducesResponseType(200)]
        public ObjectResult List()
        {
            return Ok(new
            {
                Date = DateTime.Now,
                Ip = HttpContext.Connection.RemoteIpAddress.ToString()
            });
        }
    }
}