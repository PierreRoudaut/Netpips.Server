using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Humanizer;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core;
using Netpips.Core.Service;
using Netpips.Core.Settings;
using Netpips.Download.Model;
using Netpips.Subscriptions.Model;

namespace Netpips.Download.Event
{
    public class SendItemCompletedEmail : IListener<ItemDownloaded>
    {
        private readonly ITvShowSubscriptionRepository tvShowSubscriptionRepository;
        private readonly ISmtpService smtpService;
        private readonly ILogger<SendItemCompletedEmail> logger;
        private readonly IDownloadItemRepository downloadItemRepository;
        private readonly NetpipsSettings settings;


        public SendItemCompletedEmail(ITvShowSubscriptionRepository tvShowSubscriptionRepository, ISmtpService smtpService, ILogger<SendItemCompletedEmail> logger, IDownloadItemRepository downloadItemRepository, IOptions<NetpipsSettings> options)
        {
            this.tvShowSubscriptionRepository = tvShowSubscriptionRepository;
            this.smtpService = smtpService;
            this.logger = logger;
            this.downloadItemRepository = downloadItemRepository;
            this.settings = options.Value;
        }

        public Task HandleAsync(ItemDownloaded broadcasted)
        {
            this.logger.LogInformation("SendItemCompletedEmail START");
            var item = downloadItemRepository.Find(broadcasted.DownloadItemId);
            if (tvShowSubscriptionRepository.IsSubscriptionDownload(item, out var subscribedUsersEmails))
            {
                this.logger.LogInformation("Item was downloaded by a subscription");
                this.logger.LogInformation($"Sending email to [{string.Join(",", subscribedUsersEmails)}]");
                NotifySubscribedUsers(item, subscribedUsersEmails);
            }
            else
            {
                this.logger.LogInformation($"Item was manually downloaded by {item.Owner.Email}");
                NotifyOwner(item);
            }
            this.logger.LogInformation("SendItemCompletedEmail END");
            return Task.CompletedTask;
        }

        public void NotifySubscribedUsers(DownloadItem item, List<string> subscribedUsersEmails)
        {
            if (subscribedUsersEmails.Count == 0)
            {
                return;
            }
            var email = new MailMessage
            {
                IsBodyHtml = true,
                Subject = $"[New episode available] { item.MainFilename }",
                Body = BuildNewEpisodeAvailableMailBody(item)
            };
            subscribedUsersEmails.ForEach(e => email.Bcc.Add(new MailAddress(e)));
            smtpService.Send(email);
        }

        private string BuildNewEpisodeAvailableMailBody(DownloadItem item)
        {
            // todo: embed TvMazeApi info for episode
            // todo: enhance email template with Coravel.Mailer
            // todo: add button to open file on Plex web/app

            var tvShowsUri = new Uri(settings.Domain, "tv-shows");
            var html = "<div>";
            html += $"<div>{ item.MainFilename } is available on <a href='{settings.PlexDomain}'>{settings.PlexDomain.Host}</a></div>";
            if (!item.MovedFiles.Any(f => f.Path.EndsWith(".srt")))
            {
                html += "<div style='color:grey'>Subtitles are not available yet but will be downloaded shortly.</div>";
            }
            html += $"<div>To update your list of shows: <a href='{tvShowsUri}'>{tvShowsUri.Host}</a></div>";
            html += "</div>";
            return html;
        }

        public string BuildDownloadCompletedMailBody(DownloadItem item)
        {
            var downloadedIn = item.DownloadedAt.Subtract(item.StartedAt);
            var avgSpeed = new ByteSize(item.TotalSize).Per(downloadedIn);
            var processedIn = item.CompletedAt.Subtract(item.DownloadedAt);
            var html = OsHelper.GetRessourceContent("download-completed-email.tmpl.html");
            html = html
                .Replace("{downloadedIn}", downloadedIn.Humanize())
                .Replace("{avgSpeed}", avgSpeed.Humanize("#"))
                .Replace("{processedIn}", processedIn.Humanize())
                .Replace("{movedFiles}", string.Join("",
                    item.MovedFiles.Where(pmi => pmi.Size.HasValue).OrderBy(x => x.Path).Select(pmi =>
                        "<tr>" +
                        "   <td>" + pmi.Path.Split('/').Last() + "</td>" +
                        "   <td>" + new ByteSize((double)pmi.Size).Humanize("#") + "</td>" +
                        "</tr>")
                    ));
            return html;
        }

        public void NotifyOwner(DownloadItem item)
        {
            var toAddress = new MailAddress(item.Owner.Email);
            var email = new MailMessage
            {
                To = { toAddress },
                IsBodyHtml = true,
                Subject = "[Download completed] " + item.Name,
                Body = BuildDownloadCompletedMailBody(item)
            };
            smtpService.Send(email);
        }
    }
}
