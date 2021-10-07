namespace Netpips.Media.Filebot
{
    public class RenameRequest
    {
        public string Path { get; set; }
        public string BaseDestPath { get; set; }
        public string Db { get; set; }
        public string Action { get; set; } = "test";
    }
}