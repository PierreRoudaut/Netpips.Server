using System;

namespace Netpips.Media.MediaInfo
{
    public interface IMediaInfoService
    {
        bool TryGetDuration(string path, out TimeSpan duration);
    }
}