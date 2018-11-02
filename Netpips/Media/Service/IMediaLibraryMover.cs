using System.Collections.Generic;
using System.IO;
using Netpips.Download.Model;
using Netpips.Media.Model;

namespace Netpips.Media.Service {
    public interface IMediaLibraryMover {

        /// <summary>
        /// Moves a video file to it's proper media library folder
        /// -Uses filebot to guess the file's destination
        /// -Uses video duration as fallback
        /// </summary>
        /// <param name="path">The video file to handle</param>
        /// <returns>The new path of the moved file</returns>
        List<FileSystemInfo> MoveVideoFile (string path);

        /// <summary>
        /// Moves a downloaded music file to the media library music folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        FileInfo MoveMusicFile (string path);

        /// <summary>
        /// Move and rename any downloaded subtitles to its approriate processed video file
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        List<FileInfo> MoveMatchingSubtitlesOf (string srcPath, string destPath);

        /// <summary>
        /// Compute and process a download item folder recursively
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A list of media library relative paths of the processed files </returns>
        List<MediaItem> ProcessDownloadItem (DownloadItem item);

        /// <summary>
        /// Triggers the processing of any file recursively
        /// Extract archive as well
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A list of every processed files with their new full path</returns>
        List<FileSystemInfo> ProcessDir (string path);

        /// <summary>
        /// Move to others
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        FileInfo MoveUnknownFile(string path);
    }
}