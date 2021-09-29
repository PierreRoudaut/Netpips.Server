using System.Collections.Generic;
using System.Net;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class TorrentSearchResult
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Html { get; set; }
        public string Ellapsed { get; set; }
        public List<TorrentSearchItem> Items  { get; set; }
        public bool Succeeded { get; set; }
    }
}