using System;

namespace Netpips.Core.CommandLine
{
    public class CommandLineResult
    {
        public bool Suceeded => ExitCode == 0;
        public int? ExitCode { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
        public Exception Exception { get; set; }
        public long ElapsedMs { get; set; }
    }
}