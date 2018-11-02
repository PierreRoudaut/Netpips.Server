using System.Collections.Generic;
using System.Threading.Tasks;
using Netpips.Search.Model;

namespace Netpips.Search.Service
{
    public interface ITorrentSearchScrapper
    {
        Task<List<TorrentSearchItem>> SearchAsync(string query);
    }
}