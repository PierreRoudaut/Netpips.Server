using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer.Bytes;

namespace Netpips.Download.DownloadMethod.PeerToPeer
{
    public class TransmissionItem
    {
        private readonly string summaryStats;

        private readonly string fullStats;

        public TransmissionItem(string summaryStats, string fullStats)
        {
            this.summaryStats = summaryStats;
            this.fullStats = fullStats;
        }


        public string ParseStandardValue(string property)
        {
            var standardPattern = string.Format(@"  {0}: (?<value>.*(?=\n))", property);
            var match = new Regex(standardPattern).Match(summaryStats);
            if (!match.Groups["value"].Success)
            {
                return null;
            }
            return match.Groups["value"].Value.Trim();
        }

        public int? Id
        {
            get
            {
                var value = ParseStandardValue("Id");
                if (value == null || !int.TryParse(value, out var id))
                {
                    return null;
                }
                return id;
            }
        }

        public string Name => ParseStandardValue("Name");

        public string Hash => ParseStandardValue("Hash");

        public long TotalSize
        {
            get
            {
                const string Format = @"  Total size: (?<value>.*(?=\())";
                var match = new Regex(Format).Match(summaryStats);
                if (!match.Groups["value"].Success || !ByteSize.TryParse(match.Groups["value"].Value, out var byteSize))
                {
                    return 0;
                }

                return (long)byteSize.Bytes;
            }
        }

        public long DownloadedSize
        {
            get
            {
                var value = ParseStandardValue("Downloaded");
                if (value == null || !ByteSize.TryParse(value, out var byteSize))
                {
                    return 0;
                }
                return (long)byteSize.Bytes;
            }
        }

        public string State => ParseStandardValue("State");

        public string Location => ParseStandardValue("Location");

        public string Error => ParseStandardValue("Error");

        public int Peers
        {
            get
            {
                var pattern = "  Peers: connected to 0, uploading to 0, downloading from (?<nb_peers>.*)";
                var match = new Regex(pattern).Match(summaryStats);
                if (!match.Groups["nb_peers"].Success || !int.TryParse(match.Groups["nb_peers"].Value, out var nbPeers))
                {
                    return 0;
                }
                return nbPeers;
            }
        }


        public struct File
        {
            public int Index { get; set; }
            public double PercentageProgress { get; set; }
            public double TotalSize { get; set; }
            public string Filename { get; set; }
        }

        public List<File> Files
        {
            get
            {
                var list = new List<File>();
                var pattern = @"[\t\s]+\d+:[\t\s]+(?<percent>\d+)%[\t\s]+[A-Za-z]+[\t\s]+[A-Za-z]+[\t\s]+(?<nb>\S+)[\t\s]+(?<unit>[A-Za-z]+)[\t\s]+(?<filename>.*)";
                var matchs = new Regex(pattern).Matches(fullStats).ToList();
                matchs.ForEach(m =>
                {
                    var progress = double.TryParse(m.Groups["percent"].Value, out var p) ? p : 0;
                    var nb = ByteSize.TryParse(m.Groups["nb"].Value, out var byteSize) ? byteSize.Bytes : 0;
                    list.Add(new File
                    {
                        Filename = m.Groups["filename"].Value,
                        PercentageProgress = progress,
                        TotalSize = nb
                    });
                });
                return list;
            }
        }
    }
}
