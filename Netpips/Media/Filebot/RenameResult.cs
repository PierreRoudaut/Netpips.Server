namespace Netpips.Media.Filebot
{
    public class RenameResult
    {
        public bool Succeeded { get; set; }
        public string Reason { get; set; }
        public string RawExecutedCommand { get; set; }
        public string DestPath { get; set; }
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
    }
}