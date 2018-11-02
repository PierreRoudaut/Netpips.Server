using System;
using Netpips.Core;

namespace Netpips.Media.Service
{
    public class MediaInfoService : IMediaInfoService
    {
        public bool TryGetDuration(string path, out TimeSpan duration)
        {
            duration = TimeSpan.FromMilliseconds(0);
            if (OsHelper.ExecuteCommand("mediainfo", "--Inform=\"General;%Duration%\"", out var outputDuration, out _) != 0)
            {
                return false;
            }
            if (!double.TryParse(outputDuration, out var value))
            {
                return false;
            }
            duration = TimeSpan.FromMilliseconds(value);
            return true;
        }
    }
}