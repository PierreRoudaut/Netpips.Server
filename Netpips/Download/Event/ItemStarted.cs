using Coravel.Events.Interfaces;
using Netpips.Download.Model;

namespace Netpips.Download.Event
{
    public class ItemStarted : IEvent
    {
        public DownloadItem Item { get; set; }
        public ItemStarted(DownloadItem item)
        {
            Item = item;
        }
    }
}
