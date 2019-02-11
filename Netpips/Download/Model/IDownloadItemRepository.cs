using System;
using System.Collections.Generic;

namespace Netpips.Download.Model
{
    public interface IDownloadItemRepository
    {

        /// <summary>
        /// Find all not cleaned up downloadItems
        /// </summary>
        /// <returns></returns>
        IEnumerable<DownloadItem> FindAllUnarchived();

        /// <summary>
        /// Fetch a downloadItem and calculate it's progression
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        DownloadItem Find(string token);

        DownloadItem Find(Guid id);


        void Add(DownloadItem item);

        void Update(DownloadItem item);

        void Start(DownloadItem item);
        void Archive(DownloadItem item);
        void Cancel(DownloadItem item);

        bool IsUrlDownloading(string url);

        List<DownloadItem> GetPassedItemsToArchive(int thresholdDays);

        bool HasPendingDownloads();
    }
}