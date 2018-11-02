namespace Netpips.Identity.Authorization
{
    public enum Role
    {
        User = 0,
        Admin = 1,
        SuperAdmin = 2
    }

    public static class IdentityPolicies
    {
        public const string AdminOrHigherPolicy = "AdminOrHigherPolicy";
    }
}
