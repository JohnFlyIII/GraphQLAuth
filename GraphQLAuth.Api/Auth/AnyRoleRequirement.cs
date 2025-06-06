using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GraphQLAuth.Api.Auth;

/// <summary>
/// Authorization requirement that checks if user has SystemAdmin role 
/// OR any client role (ClientOwner/ClientUser)
/// Used for non-client-specific resources
/// </summary>
public class AnyRoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Optional: Specific roles required. If empty, any client role is acceptable
    /// </summary>
    public string[] RequiredRoles { get; }

    public AnyRoleRequirement(params string[] roles)
    {
        RequiredRoles = roles ?? Array.Empty<string>();
    }
}

/// <summary>
/// Authorization handler for any role requirement
/// </summary>
public class AnyRoleAuthorizationHandler : AuthorizationHandler<AnyRoleRequirement>
{
    private readonly IAuthorizationService _authService;
    private readonly ILogger<AnyRoleAuthorizationHandler> _logger;

    public AnyRoleAuthorizationHandler(
        IAuthorizationService authService, 
        ILogger<AnyRoleAuthorizationHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyRoleRequirement requirement)
    {
        var user = context.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("Unauthenticated user attempting to access resource requiring any role");
            context.Fail();
            return Task.CompletedTask;
        }

        // SystemAdmin always has access
        if (_authService.IsSystemAdmin(user))
        {
            _logger.LogDebug("SystemAdmin {UserId} granted access (any role requirement)", user.Identity.Name);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Get all client roles for the user
        var clientRoles = _authService.GetClientRoles(user);
        
        if (!clientRoles.Any())
        {
            _logger.LogWarning("User {UserId} denied access - no client roles found", user.Identity.Name);
            context.Fail();
            return Task.CompletedTask;
        }

        // If specific roles are required, check for those
        if (requirement.RequiredRoles.Length > 0)
        {
            var hasRequiredRole = clientRoles.Any(cr => requirement.RequiredRoles.Contains(cr.RoleId));
            if (hasRequiredRole)
            {
                _logger.LogDebug("User {UserId} granted access - has required role from: [{RequiredRoles}]", 
                    user.Identity.Name, string.Join(", ", requirement.RequiredRoles));
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} denied access - missing required roles: [{RequiredRoles}], user has: [{UserRoles}]", 
                    user.Identity.Name, 
                    string.Join(", ", requirement.RequiredRoles),
                    string.Join(", ", clientRoles.Select(cr => cr.RoleId)));
                context.Fail();
            }
        }
        else
        {
            // Any client role is acceptable
            _logger.LogDebug("User {UserId} granted access - has client roles: [{UserRoles}]", 
                user.Identity.Name, string.Join(", ", clientRoles.Select(cr => cr.RoleId)));
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}