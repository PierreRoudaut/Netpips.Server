namespace Netpips.Download.Exception
{
    public class StartDownloadException : System.Exception
    {
        public StartDownloadException(string message)
        {
            Message = message;
        }
        public override string Message { get; }
    }
}