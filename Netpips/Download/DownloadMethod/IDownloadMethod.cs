using Netpips.Download.Exception;
using Netpips.Download.Model;

namespace Netpips.Download.DownloadMethod
{
    public interface IDownloadMethod
    {
        /// <summary>
        /// Calculate the downloaded length of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        long GetDownloadedSize(DownloadItem item);

        /// <summary>
        /// Process the different necessary action to properly stop the download of an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Cancel(DownloadItem item);

        /// <summary>
        /// Attempts to start a download for a given url
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="FileNotDownloadableException">If fileUrl does not point to a downloadable file</exception>
        /// <exception cref="StartDownloadException">An unexpected error occured while starting the download</exception>
        /// <returns>The populated download item</returns>
        void Start(DownloadItem item);
        bool Archive(DownloadItem item);

        bool CanHandle(string fileUrl);
        bool CanHandle(DownloadType type);
    }
}