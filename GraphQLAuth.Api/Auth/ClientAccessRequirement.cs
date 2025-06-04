using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GraphQLAuth.Api.Auth;

public class ClientAccessRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }
    
    public ClientAccessRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

public class ClientAccessHandler : AuthorizationHandler<ClientAccessRequirement>
{
    private readonly IAuthorizationService _authService;
    
    public ClientAccessHandler(IAuthorizationService authService)
    {
        _authService = authService;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientAccessRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // System admin always has access
        if (_authService.IsSystemAdmin(context.User))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has any of the allowed roles for any client
        var clientRoles = _authService.GetClientRoles(context.User);
        if (clientRoles.Any(cr => requirement.AllowedRoles.Contains(cr.RoleId)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}