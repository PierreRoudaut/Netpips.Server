using System;
using System.IO;
using System.Linq;
using Coravel.Events.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Settings;
using Netpips.Download.Controller;
using Netpips.Download.DownloadMethod;
using Netpips.Download.Event;
using Netpips.Download.Exception;
using Netpips.Download.Model;

namespace Netpips.Download.Service
{
    public class DownloadItemService : IDownloadItemService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DownloadItemService> logger;
        private readonly NetpipsSettings settings;
        private readonly IDispatcher dispatcher;
        private readonly IDownloadItemRepository repository;

        public DownloadItemService(ILogger<DownloadItemService> logger, IServiceProvider serviceProvider, IOptions<NetpipsSettings> options, IDispatcher dispatcher, IDownloadItemRepository repository)
        {
            settings = options.Value;
            this.serviceProvider = serviceProvider;
            this.dispatcher = dispatcher;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Starts a download
        /// </summary>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool StartDownload(DownloadItem item, out DownloadItemActionError error)
        {
            error = DownloadItemActionError.DownloadabilityFailure;

            var downloadMethod = serviceProvider.GetServices<IDownloadMethod>().FirstOrDefault(x => x.CanHandle(item.FileUrl));
            if (downloadMethod == null)
            {
                logger.LogWarning("URL not handled: " + item.FileUrl);
                error = DownloadItemActionError.UrlNotHandled;
                return false;
            }

            try
            {
                downloadMethod.Start(item);
                item.DownloadedSize = 0;
                item.StartedAt = DateTime.Now;
                item.Archived = false;
                item.State = DownloadState.Downloading;

                this.repository.Add(item);
                _ = dispatcher.Broadcast(new ItemStarted(item));
            }
            catch (FileNotDownloadableException ex)
            {
                logger.LogWarning(ex.Message);
                error = DownloadItemActionError.DownloadabilityFailure;
                return false;
            }
            catch (StartDownloadException ex)
            {
                logger.LogWarning(ex.Message);
                error = DownloadItemActionError.StartDownloadFailure;
                return false;
            }
            return true;
        }


        public void CancelDownload(DownloadItem item)
        {
            logger.LogInformation("Canceling " + item.Name);
            var downloadMethod = serviceProvider.GetServices<IDownloadMethod>().First(x => x.CanHandle(item.Type));

            downloadMethod.Cancel(item);
            repository.Cancel(item);
        }

        public void ArchiveDownload(DownloadItem item)
        {
            logger.LogInformation("Archiving " + item.Name);
            var downloadMethod = serviceProvider.GetServices<IDownloadMethod>().First(x => x.CanHandle(item.Type));
            downloadMethod.Archive(item);
            repository.Archive(item);

            var dirInfo = new DirectoryInfo(Path.Combine(settings.DownloadsPath, item.Token));
            if (dirInfo.Exists)
            {
                dirInfo.Delete(true);
            }

        }

        public void ComputeDownloadProgress(DownloadItem item)
        {
            var downloadMethod = serviceProvider.GetServices<IDownloadMethod>().First(x => x.CanHandle(item.Type));
            item.DownloadedSize = downloadMethod.GetDownloadedSize(item);
        }
    }

}