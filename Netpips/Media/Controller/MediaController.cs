using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netpips.Media.Model;
using Netpips.Media.Service;

namespace Netpips.Media.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<MediaController> logger;

        private readonly IMediaLibraryService mediaLibraryService;

        private readonly IMediaItemRepository repository;

        public MediaController(ILogger<MediaController> logger, IMediaLibraryService mediaLibraryService, IMediaItemRepository repository)
        {
            this.logger = logger;
            this.mediaLibraryService = mediaLibraryService;
            this.repository = repository;
        }

        [ProducesResponseType(typeof(MediaFolderSummary), 200)]
        [HttpGet("diskUsage", Name = "DiskUsage")]
        public ObjectResult DiskUsage()
        {
            var drive = DriveInfo.GetDrives().OrderByDescending(x => x.TotalSize).First();
            return Ok(new { drive.TotalSize, drive.AvailableFreeSpace });
        }

        [ProducesResponseType(typeof(IEnumerable<MediaFolderSummary>), 200)]
        [HttpGet("libraryDistribution", Name = "LibraryDistribution")]
        public IActionResult MediaLibraryDistribution() => Ok(repository.GetMediaLibraryRootFolderDistribution());

        [ProducesResponseType(typeof(IEnumerable<PlainMediaItem>), 200)]
        [HttpGet("libraryPlain", Name = "LibraryPlain")]
        public IActionResult MediaLibraryPlain() => Ok(repository.FindAll());

        public class SubtitleParameters
        {
            public string Path { get; set; }
            public string Lang { get; set; }
        }

        [HttpPost("getSubtitles", Name = "GetSubtitles")]
        [ProducesResponseType(typeof(PlainMediaItem), 200)]
        [ProducesResponseType(typeof(PlainMediaItem), 204)]
        [ProducesResponseType(404)]
        public IActionResult GetSubtitles([FromBody] SubtitleParameters parameters)
        {
            var item = repository.Find(parameters.Path);
            if (item == null)
            {
                return StatusCode(404, null);
            }

            var srtItem = mediaLibraryService.GetSubtitles(item, parameters.Lang);
            if (srtItem == null)
            {
                return StatusCode(204, null);
            }
            return StatusCode(200, srtItem);
        }


        public class RenameParameters
        {
            public string Path { get; set; }
            public string NewName { get; set; }
        }

        [HttpPost("rename", Name = "Rename")]
        [ProducesResponseType(typeof(PlainMediaItem), 200)]
        [ProducesResponseType(400)]
        public ObjectResult Rename([FromBody] RenameParameters parameters)
        {
            var item = repository.Find(parameters.Path);
            if (item == null)
            {
                return StatusCode(404, null);
            }

            try
            {
                item.Rename(parameters.NewName);
            }
            catch (Exception)
            {
                return StatusCode(400, null);
            }
            return StatusCode(200, item);
        }

        [HttpPost("autoRename", Name = "Auto rename")]
        [ProducesResponseType(typeof(IEnumerable<PlainMediaItem>), 200)]
        [ProducesResponseType(400)]
        public ObjectResult AutoRename([FromBody] string path)
        {
            var item = repository.Find(path);
            if (item == null)
            {
                return StatusCode(404, null);
            }

            var renamedItems = mediaLibraryService.AutoRename(item);
            if (renamedItems == null)
            {
                return StatusCode(400, null);
            }

            return StatusCode(200, renamedItems);
        }

        [HttpPost("delete")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        public ObjectResult Delete([FromBody] string path)
        {
            var item = repository.Find(path);
            if (item == null)
            {
                return StatusCode(404, null);
            }

            try
            {
                item.Delete();
            }
            catch (Exception)
            {
                return StatusCode(400, null);
            }

            return StatusCode(200, true);
        }
    }
}