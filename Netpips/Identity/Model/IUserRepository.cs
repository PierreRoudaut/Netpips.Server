using System;
using System.Collections.Generic;

namespace Netpips.Identity.Model
{
    public interface IUserRepository
    {
        User GetDaemonUser();
        User FindUser(string email);
        User FindUser(Guid id);
        void DeleteUser(User user);

        List<User> GetUsers(Guid userToExclude);

        bool UpdateUser(User user);

        bool CreateUser(User user);

        bool IsTvShowSubscribedByOtherUsers(int showRssId, Guid id);
    }
}
