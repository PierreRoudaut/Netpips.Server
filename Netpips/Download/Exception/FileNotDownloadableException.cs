namespace Netpips.Download.Exception
{
    public class FileNotDownloadableException : System.Exception
    {
        public FileNotDownloadableException(string message)
        {
            Message = message;
        }
        public override string Message { get; }
    }
}