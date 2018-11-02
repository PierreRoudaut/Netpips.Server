using System.Threading.Tasks;

namespace Netpips.Download.DownloadMethod.DirectDownload
{
    public interface IDirectDownloadTask
    {
        void Cancel();
        Task StartAsync();
    }
}