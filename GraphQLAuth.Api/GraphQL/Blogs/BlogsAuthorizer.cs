using System.Security.Claims;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Models;
using Npgsql.Replication;

public class BlogsAuthorizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authService;

    public BlogsAuthorizer(IHttpContextAccessor httpContextAccessor, IAuthorizationService authService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
    }

    public IQueryable<Blog> AuthorizeFilter(IQueryable<Blog> query)
    {
        var user = (_httpContextAccessor.HttpContext?.User) ?? throw new Exception("user is null");
        // No change if admin
        if (_authService.IsSystemAdmin(user))
        {
            return query;
        }

        // Filter to only clients they have access to
        var clientRoles = _authService.GetClientRoles(user);
        var allowedClientIds = clientRoles
            .Where(cr => cr.RoleId == AuthConstants.Roles.ClientOwner || cr.RoleId == AuthConstants.Roles.ClientUser)
            .Select(cr => cr.ClientId)
            .Distinct()
            .ToList();

        return query.Where(b => allowedClientIds.Contains(b.ClientId));

    }
}