using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GraphQLAuth.Api.Auth;

public class ClientTenantRequirement : IAuthorizationRequirement
{
    public bool RequireExplicitClientAccess { get; }

    public ClientTenantRequirement(bool requireExplicitClientAccess = true)
    {
        RequireExplicitClientAccess = requireExplicitClientAccess;
    }
}

public class ClientTenantRequirementHandler : AuthorizationHandler<ClientTenantRequirement>
{
    private readonly IAuthorizationService _authService;

    public ClientTenantRequirementHandler(IAuthorizationService authService)
    {
        _authService = authService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ClientTenantRequirement requirement)
    {
        var user = context.User;

        // SystemAdmin always passes
        if (_authService.IsSystemAdmin(user))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // If we require explicit client access, user must have at least one client role
        if (requirement.RequireExplicitClientAccess)
        {
            var clientRoles = _authService.GetClientRoles(user);
            var hasClientAccess = clientRoles.Any(cr => 
                cr.RoleId == AuthConstants.Roles.ClientOwner || 
                cr.RoleId == AuthConstants.Roles.ClientUser);

            if (hasClientAccess)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}