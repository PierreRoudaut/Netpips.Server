using System;
using System.Net;

namespace Netpips.Core.Http
{
    public class HttpResponseLite
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Html { get; set; }
        public long ElapsedMs { get; set; }
        public Exception Exception { get; set; }
        public bool IsSuccessStatusCode => (int) StatusCode >= 200 && (int) StatusCode <= 299;
    }
}