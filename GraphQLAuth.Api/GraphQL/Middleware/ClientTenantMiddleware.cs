using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using GraphQLAuth.Api.Auth;
using System.Security.Claims;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.GraphQL.Middleware;

public class ClientTenantMiddleware
{
    private readonly FieldDelegate _next;
    private readonly IAuthorizationService _authService;

    public ClientTenantMiddleware(FieldDelegate next, IAuthorizationService authService)
    {
        _next = next;
        _authService = authService;
    }

    public async ValueTask InvokeAsync(IMiddlewareContext context)
    {
        // Get the user from context
        var user = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            // If not SystemAdmin, inject tenant scoping
            if (!_authService.IsSystemAdmin(user))
            {
                var clientRoles = _authService.GetClientRoles(user);
                var allowedClientIds = clientRoles
                    .Where(cr => cr.RoleId == AuthConstants.Roles.ClientOwner || 
                                cr.RoleId == AuthConstants.Roles.ClientUser)
                    .Select(cr => cr.ClientId)
                    .Distinct()
                    .ToList();

                // Store allowed client IDs in context for use by resolvers
                context.SetScopedState("AllowedClientIds", allowedClientIds);
                context.SetScopedState("IsSystemAdmin", false);
            }
            else
            {
                context.SetScopedState("IsSystemAdmin", true);
            }
        }

        await _next(context);
    }
}

public static class ClientTenantMiddlewareExtensions
{
    public static IObjectFieldDescriptor UseClientTenantFilter(this IObjectFieldDescriptor descriptor)
    {
        return descriptor.Use<ClientTenantMiddleware>();
    }
}