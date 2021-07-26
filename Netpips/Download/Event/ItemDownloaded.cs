using System;
using Coravel.Events.Interfaces;

namespace Netpips.Download.Event
{
    public class ItemDownloaded : IEvent
    {
        public Guid DownloadItemId { get; set; }
        public ItemDownloaded(Guid downloadItemId)
        {
            DownloadItemId = downloadItemId;
        }
    }
}
