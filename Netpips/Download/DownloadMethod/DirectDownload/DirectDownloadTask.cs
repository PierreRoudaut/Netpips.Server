using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Netpips.Core;
using Netpips.Download.Event;
using Netpips.Download.Model;

namespace Netpips.Download.DownloadMethod.DirectDownload
{
    public class DirectDownloadTask : IDirectDownloadTask
    {

        private static readonly HttpClientHandler Handler = new HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = true
        };
        private static readonly HttpClient Client = new HttpClient(Handler);

        private readonly DownloadItem item;
        private readonly IDispatcher dispatcher;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly string downloadDestPath;
        private readonly List<string> cookies;

        public DirectDownloadTask(DownloadItem item, IDispatcher dispatcher, string downloadDestPath, List<string> cookies)
        {
            this.item = item;
            this.dispatcher = dispatcher;
            this.downloadDestPath = downloadDestPath;
            this.cookies = cookies;
            Client.DefaultRequestHeaders.Add("User-Agent", OsHelper.UserAgent);
        }

        public async Task StartAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, item.FileUrl);
            request.Headers.Add("Cookie", cookies);

            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var resStream = await response.Content.ReadAsStreamAsync();
            var fStream = File.Create(downloadDestPath);

            var downloadCompleted = false;

            await resStream
                .CopyToAsync(fStream, 81920, tokenSource.Token)
                .ContinueWith(task => downloadCompleted = task.IsCompletedSuccessfully);

            resStream.Dispose();
            fStream.Dispose();
            DirectDownloadMethod.DirectDownloadTasks.TryRemove(item.Token, out _);
            if (downloadCompleted)
            {
                _ = dispatcher.Broadcast(new ItemDownloaded(item.Id));
            }
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }
    }

}