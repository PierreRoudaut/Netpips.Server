using Netpips.Identity.Authorization;

namespace Netpips.Identity.Service
{
    public interface IUserAdministrationService
    {
        bool CanUpdate(Role actor, Role subject, Role subjectNewRole);
        bool CanUpdate(Role actor, Role subject);
    }
}