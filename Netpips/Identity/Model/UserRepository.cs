using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Model;
using Netpips.Core.Settings;
using Netpips.Identity.Authorization;

namespace Netpips.Identity.Model
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> logger;
        private readonly AppDbContext dbContext;
        private readonly NetpipsSettings settings;

        public UserRepository(ILogger<UserRepository> logger, AppDbContext dbContext, IOptions<NetpipsSettings> options)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.settings = options.Value;
        }

        public User GetDaemonUser()
        {
            return this.dbContext.Users.First(u => u.Email == settings.DaemonUserEmail);
        }

        public User FindUser(string email) => dbContext.Users
            .Include(i => i.TvShowSubscriptions)
            .FirstOrDefault(u => email == u.Email);

        public User FindUser(Guid id) => dbContext.Users
            .Include(i => i.TvShowSubscriptions)
            .FirstOrDefault(u => id == u.Id);

        public void DeleteUser(User user)
        {
            this.dbContext.Users.Remove(user);
            this.dbContext.SaveChanges();
        }

        public List<User> GetUsers(Guid userToExclude)
        {
            return dbContext.Users
                .Where(u => u.Role != Role.SuperAdmin && u.Id != userToExclude)
                .OrderBy(u => u.Email)
                .ToList();
        }

        public bool UpdateUser(User user)
        {
            try
            {
                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError("Failed to update user: " + user.Email);
                logger.LogError(ex.Message);
                return false;
            }
            return true;
        }

        public bool CreateUser(User user)
        {
            try
            {
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError("Failed to add user: " + user.Email);
                logger.LogError(ex.Message);
                return false;
            }
            return true;
        }

        public bool IsTvShowSubscribedByOtherUsers(int showRssId, Guid id) => 
            dbContext.Users
                .Where(u => u.Id != id)
                .Any(u => u.TvShowSubscriptions.Any(s => s.ShowRssId == showRssId));

    }
}
