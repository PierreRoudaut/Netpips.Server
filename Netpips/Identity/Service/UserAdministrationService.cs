using Netpips.Identity.Authorization;

namespace Netpips.Identity.Service
{
    public class UserAdministrationService : IUserAdministrationService
    {
        public bool CanUpdate(Role actor, Role subject) => subject < actor;

        public bool CanUpdate(Role actor, Role subject, Role subjectNewRole) => CanUpdate(actor, subject) && subjectNewRole < actor;
    }
}