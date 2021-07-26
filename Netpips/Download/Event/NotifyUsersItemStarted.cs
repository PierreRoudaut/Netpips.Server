using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Netpips.Download.Event
{
    public class NotifyUsersItemStarted : IListener<ItemStarted>
    {
        private readonly ILogger<NotifyUsersItemStarted> logger;
        public NotifyUsersItemStarted(ILogger<NotifyUsersItemStarted> logger)
        {
            this.logger = logger;
        }
        public Task HandleAsync(ItemStarted broadcasted)
        {
            logger.LogInformation("[HandleAsync] handling DownloadItemStarted event for: " + broadcasted.Item.Name);
            //todo: send push notification with SignalR
            return Task.CompletedTask;
        }
    }
}
