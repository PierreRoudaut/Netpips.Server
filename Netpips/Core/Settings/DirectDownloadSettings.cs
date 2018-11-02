using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Netpips.Core.Settings
{
    public class FileHosterInfo
    {
        public string Name { get; set; }
        public Uri LoginUrl { get; set; }
        public string Pattern { get; set; }
        public Dictionary<string, string> CredentialsData { get; set; }
        public bool CanHandle(string url) => new Regex(Pattern).IsMatch(url);
    }

    public class DirectDownloadSettings
    {
        public List<FileHosterInfo> Filehosters { get; set; }
    }
}
