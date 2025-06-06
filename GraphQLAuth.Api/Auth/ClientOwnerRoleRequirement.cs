using Microsoft.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.Auth;

public class ClientOwnerRoleRequirement : IAuthorizationRequirement
{
}

public class ClientOwnerRoleHandler : AuthorizationHandler<ClientOwnerRoleRequirement, IResolverContext>
{
    private readonly IAuthorizationService _authService;

    public ClientOwnerRoleHandler(IAuthorizationService authService)
    {
        _authService = authService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientOwnerRoleRequirement requirement,
        IResolverContext resolverContext)
    {
        var user = context.User;
        
        // Always succeed to allow the field to be queried
        // But store the authorization result for the resolver to use
        bool hasAccess = false;
        
        // System admins always have access
        if (_authService.IsSystemAdmin(user))
        {
            hasAccess = true;
        }
        else
        {
            // Get the parent resource object from resolver context
            var parentResource = resolverContext.Parent<IClientResource>();
            if (parentResource != null)
            {
                // Check if user has ClientOwner role for this specific resource's client
                hasAccess = _authService.HasClientRole(user, parentResource.ClientId, AuthConstants.Roles.ClientOwner);
            }
        }

        // Store the result in the resolver context for the resolver to use
        resolverContext.SetScopedState("ClientOwnerRoleAccess", hasAccess);
        
        // Always succeed so the field can be queried
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}