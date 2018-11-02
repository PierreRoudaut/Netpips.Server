using System.Linq;
using Coravel.Events.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netpips.Core.Service;
using Netpips.Download.Authorization;
using Netpips.Download.DownloadMethod.PeerToPeer;
using Netpips.Download.Event;
using Netpips.Download.Model;

namespace Netpips.Download.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public class TorrentDoneController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<TorrentDoneController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly ITorrentDaemonService torrentDaemonService;
        private readonly IDispatcher dispatcher;
        private readonly IDownloadItemRepository repository;

        public TorrentDoneController(ILogger<TorrentDoneController> logger, 
                                     IDownloadItemRepository repository,
                                     IAuthorizationService authorizationService,
                                     ITorrentDaemonService torrentDaemonService,
            IDispatcher dispatcher)
        {
            this.logger = logger;
            this.authorizationService = authorizationService;
            this.torrentDaemonService = torrentDaemonService;
            this.repository = repository;
            this.dispatcher = dispatcher;
        }

        [HttpGet("{hash}", Name = "TorrentDone")]
        [ProducesResponseType(typeof(DownloadItemActionError), 404)]
        [ProducesResponseType(typeof(DownloadItemActionError), 400)]
        [ProducesResponseType(200)]
        public ObjectResult TorrentDone([FromServices] IControllerHelperService helper, string hash)
        {
            logger.LogInformation("TorrentDone: " + hash);
            if (!helper.IsLocalCall(HttpContext))
            {
                logger.LogWarning("Request not sent from the server");
                return StatusCode(403, null);
            }

            var item = repository.FindAllUnarchived().FirstOrDefault(x => x.Hash == hash);
            if (item == null)
            {
                logger.LogInformation(hash + ": note found");
                return StatusCode(404, DownloadItemActionError.ItemNotFound);
            }

            var authorizationResult = authorizationService.AuthorizeAsync(User, item, DownloadItemPolicies.TorrentDonePolicy).Result;
            if (!authorizationResult.Succeeded)
            {
                var requirement = authorizationResult.Failure.FailedRequirements.First() as DownloadItemBaseRequirement;
                return StatusCode(requirement.HttpCode, requirement.Error);
            }

            torrentDaemonService.StopTorrent(hash);
            _ = dispatcher.Broadcast(new ItemDownloaded(item.Id));

            return StatusCode(200, new { Processed = true });
        }
    }

}