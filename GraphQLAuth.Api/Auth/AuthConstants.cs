namespace GraphQLAuth.Api.Auth;

public static class AuthConstants
{
    public const string ClientRolesClaim = "client_roles";
    public const string ClientIdClaim = "client_id";
    
    public static class Roles
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string ClientOwner = "ClientOwner";
        public const string ClientUser = "ClientUser";
    }
    
    public static class Policies
    {
        public const string RequireBlogOwnerNotesAccess = "RequireBlogOwnerNotesAccess";
        public const string RequireClientTenant = "RequireClientTenant";
        public const string RequireAssetDataAccess = "RequireAssetDataAccess";
        public const string RequireAnyRole = "RequireAnyRole";
        public const string RequireSystemAdmin = "RequireSystemAdmin";
    }
}