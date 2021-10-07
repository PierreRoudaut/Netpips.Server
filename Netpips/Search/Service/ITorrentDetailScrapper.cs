using System;
using System.Threading.Tasks;

namespace Netpips.Search.Service
{
    public interface ITorrentDetailScrapper
    {
        /// <summary>
        /// Scrape and parse the magnet url prest at the torrentDetailUrl location
        /// </summary>
        /// <param name="torrentDetailUrl"></param>
        /// <returns></returns>
        Task<string> ScrapeTorrentUrlAsync(string torrentDetailUrl);

        /// <summary>
        /// Handles 
        /// </summary>
        /// <param name="torrentDetailUri"></param>
        /// <returns></returns>
        bool CanScrape(Uri torrentDetailUri);
    }
}