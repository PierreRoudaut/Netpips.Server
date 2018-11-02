using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Netpips.Core.Model;
using Netpips.Identity.Model;

namespace Netpips.Download.Model
{
    public class DownloadItemRepository : IDownloadItemRepository
    {

        private readonly ILogger<DownloadItemRepository> logger;
        private readonly AppDbContext dbContext;

        public DownloadItemRepository(ILogger<DownloadItemRepository> logger, AppDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        /// <summary>
        /// Find all not cleaned up downloadItems
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DownloadItem> FindAllUnarchived()
        {
            return dbContext.DownloadItems.Include(x => x.Owner).Where(x => !x.Archived).OrderBy(x => x.StartedAt);
        }

        public void Start(DownloadItem item)
        {
            item.Archived = false;
            item.StartedAt = DateTime.Now;
            item.State = DownloadState.Downloading;
            dbContext.SaveChanges();
            dbContext.Entry(item).Reference(c => c.Owner).Load();
        }

        public void Archive(DownloadItem item)
        {
            item.Archived = true;
            dbContext.Entry(item).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        public void Cancel(DownloadItem item)
        {
            item.CanceledAt = DateTime.Now;
            item.State = DownloadState.Canceled;
            dbContext.Entry(item).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Verifies if a given url is currently downloading
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsUrlDownloading(string url)
        {
            return this.dbContext.DownloadItems.Include(x => x.Owner)
                .Where(x => !x.Archived)
                .Any(x => x.FileUrl == url);
        }


        public List<DownloadItem> GetPassedItemsToArchive(int thresholdDays)
        {
            var thresholdDate = DateTime.Now.AddDays(-1 * thresholdDays);

            var toArchive = this.dbContext.DownloadItems.Where(
                d => !d.Archived && ((d.State == DownloadState.Canceled && d.CanceledAt < thresholdDate)
                                     || d.State == DownloadState.Completed && d.CompletedAt < thresholdDate)).ToList();

            return toArchive;
        }

        /// <inheritdoc />
        /// <summary>
        /// Fetch a downloadItem and calculate it's progression
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public DownloadItem Find(string token)
        {
            return dbContext.DownloadItems
                .Include(x => x.Owner)
                .FirstOrDefault(x => x.Token == token);
        }

        public DownloadItem Find(Guid id)
        {
            return dbContext.DownloadItems
                .Include(x => x.Owner)
                .FirstOrDefault(x => x.Id == id);
        }

        public void Add(DownloadItem item)
        {
            dbContext.Add(item);
            dbContext.SaveChanges();
            dbContext.Entry(item).Reference(c => c.Owner).Load();
        }

        public void Update(DownloadItem item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
    }
}