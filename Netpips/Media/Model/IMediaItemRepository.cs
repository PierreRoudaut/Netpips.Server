using System.Collections.Generic;

namespace Netpips.Media.Model
{
    public interface IMediaItemRepository
    {
        /// <summary>
        /// Find all media items
        /// </summary>
        /// <returns></returns>
        IEnumerable<PlainMediaItem> FindAll();

        /// <summary>
        /// Fetch a mediaItem
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        PlainMediaItem Find(string path);

        IEnumerable<MediaFolderSummary> GetMediaLibraryRootFolderDistribution();
    }
}