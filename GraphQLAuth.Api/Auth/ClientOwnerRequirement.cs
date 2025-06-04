using Microsoft.AspNetCore.Authorization;

namespace GraphQLAuth.Api.Auth;

public class ClientOwnerRequirement : IAuthorizationRequirement
{
    public ClientOwnerRequirement()
    {
    }
}

public class ClientOwnerHandler : AuthorizationHandler<ClientOwnerRequirement>
{
    private readonly IAuthorizationService _authService;

    public ClientOwnerHandler(IAuthorizationService authService)
    {
        _authService = authService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ClientOwnerRequirement requirement)
    {
        var user = context.User;
        
        // System admins always pass
        if (_authService.IsSystemAdmin(user))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has any client owner roles
        var clientRoles = _authService.GetClientRoles(user);
        if (clientRoles.Any(cr => cr.RoleId == AuthConstants.Roles.ClientOwner))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}