using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netpips.Core.Extensions;
using Netpips.Download.Authorization;
using Netpips.Download.Model;
using Netpips.Download.Service;

namespace Netpips.Download.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class DownloadItemController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<DownloadItemController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly IDownloadItemRepository repository;
        private readonly IDownloadItemService service;

        public DownloadItemController(ILogger<DownloadItemController> logger, IDownloadItemService service, IAuthorizationService authorizationService, IDownloadItemRepository repository)
        {
            this.logger = logger;
            this.service = service;
            this.authorizationService = authorizationService;
            this.repository = repository;
        }

        [HttpGet("", Name = "GetItems")]
        [ProducesResponseType(typeof(List<DownloadItem>), 200)]
        public ObjectResult List()
        {
            var list = repository.FindAllUnarchived().ToList();
            this.logger.LogInformation($"Listing {list.Count} items");
            return Ok(list);
        }

        [HttpGet("{token}", Name = "GetItem")]
        [ProducesResponseType(typeof(DownloadItem), 200)]
        [ProducesResponseType(404)]
        public ObjectResult Get([Required] string token)
        {
            var item = repository.Find(token);
            if (item == null)
                return StatusCode(404, DownloadItemActionError.ItemNotFound);

            service.ComputeDownloadProgress(item);
            return Ok(item);
        }

        [HttpPost("start", Name = "Start")]
        [ProducesResponseType(typeof(DownloadItem), 201)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public ObjectResult Start([FromBody] string fileUrl)
        {
            if (repository.FindAllUnarchived().Any(x => x.FileUrl == fileUrl))
            {
                logger.LogWarning(fileUrl + ": url already exists");
                return StatusCode(400, DownloadItemActionError.DuplicateDownload);
            }

            var item = new DownloadItem
            {
                OwnerId = User.GetId(),
                FileUrl = fileUrl
            };

            if (!service.StartDownload(item, out var error))
            {
                switch (error)
                {
                    case DownloadItemActionError.StartDownloadFailure:
                        return StatusCode(500, error);
                    case DownloadItemActionError.DownloadabilityFailure:
                        return StatusCode(404, error);
                    case DownloadItemActionError.UrlNotHandled:
                        return StatusCode(400, error);
                }
            }

            return Created("/start", item);
        }


        [HttpPost("isUrlSupported", Name = "IsUrlSupported")]
        [ProducesResponseType(typeof(DownloadItem), 201)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public ObjectResult IsUrlSupported([FromBody] string fileUrl)
        {
            var result = this.service.ValidateUrl(fileUrl);
            return Ok(result);
        }


        [HttpPost("cancel", Name = "Cancel")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(DownloadItem), 200)]
        public ObjectResult Cancel([FromBody] string token)
        {
            var downloadItem = repository.Find(token);
            if (downloadItem == null)
            {
                return StatusCode(404, DownloadItemActionError.ItemNotFound);
            }

            var authorizationResult = authorizationService.AuthorizeAsync(User, downloadItem, DownloadItemPolicies.CancelPolicy).Result;
            if (!authorizationResult.Succeeded)
            {
                var requirement = authorizationResult.Failure.FailedRequirements.First() as DownloadItemBaseRequirement;
                logger.LogWarning(requirement.Error.ToString());
                return StatusCode(requirement.HttpCode, requirement.Error);
            }

            service.CancelDownload(downloadItem);
            return Ok(downloadItem);
        }

        [HttpPost("archive", Name = "Archive")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(200)]
        public ObjectResult Archive([FromBody] string token)
        {
            var downloadItem = repository.Find(token);
            if (downloadItem == null)
            {
                return StatusCode(404, DownloadItemActionError.ItemNotFound);
            }

            var authorizationResult = authorizationService.AuthorizeAsync(User, downloadItem, DownloadItemPolicies.ArchivePolicy).Result;
            if (!authorizationResult.Succeeded)
            {
                var requirement = authorizationResult.Failure.FailedRequirements.First() as DownloadItemBaseRequirement;
                logger.LogWarning(requirement.Error.ToString());
                return StatusCode(requirement.HttpCode, requirement.Error);
            }

            service.ArchiveDownload(downloadItem);

            return StatusCode(200, null);
        }
    }
}