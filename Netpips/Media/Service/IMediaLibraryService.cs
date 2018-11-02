using System.Collections.Generic;
using Netpips.Media.Model;

namespace Netpips.Media.Service
{
    public interface IMediaLibraryService
    {
        IEnumerable<PlainMediaItem> AutoRename(PlainMediaItem item);
        PlainMediaItem GetSubtitles(PlainMediaItem item, string lang);
    }
}