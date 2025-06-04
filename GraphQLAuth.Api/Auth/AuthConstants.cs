namespace GraphQLAuth.Api.Auth;

public static class AuthConstants
{
    public const string ClientRolesClaim = "client_roles";
    
    public static class Roles
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string ClientOwner = "ClientOwner";
        public const string ClientUser = "ClientUser";
    }
    
    public static class Policies
    {
        public const string RequireAuthenticated = "RequireAuthenticated";
        public const string RequireSystemAdmin = "RequireSystemAdmin";
        public const string RequireClientOwnerOrAdmin = "RequireClientOwnerOrAdmin";
        public const string RequireBlogOwnerNotesAccess = "RequireBlogOwnerNotesAccess";
    }
}