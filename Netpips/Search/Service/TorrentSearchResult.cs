using System.Collections.Generic;
using Netpips.Core.Http;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public class TorrentSearchResult
    {
        public HttpResponseLite Response { get; set; }
        public List<TorrentSearchItem> Items  { get; set; }
        public bool Succeeded { get; set; }
    }
}