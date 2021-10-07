using System.Threading.Tasks;

namespace Netpips.Search.Service
{
    public interface ITorrentSearchScrapper
    {
        Task<TorrentSearchResult> SearchAsync(string query);
    }
}