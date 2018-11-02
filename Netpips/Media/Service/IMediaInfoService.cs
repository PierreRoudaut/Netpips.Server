using System;

namespace Netpips.Media.Service {
    public interface IMediaInfoService {
        bool TryGetDuration (string path, out TimeSpan duration);
    }
}