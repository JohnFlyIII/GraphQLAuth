using Microsoft.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.Auth;

public class BlogOwnerNotesRequirement : IAuthorizationRequirement
{
}

public class BlogOwnerNotesHandler : AuthorizationHandler<BlogOwnerNotesRequirement, IResolverContext>
{
    private readonly IAuthorizationService _authService;

    public BlogOwnerNotesHandler(IAuthorizationService authService)
    {
        _authService = authService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BlogOwnerNotesRequirement requirement,
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
            // Get the parent Blog object from resolver context
            var blog = resolverContext.Parent<Blog>();
            if (blog != null)
            {
                // Check if user has ClientOwner role for this specific blog's client
                hasAccess = _authService.HasClientRole(user, blog.ClientId, AuthConstants.Roles.ClientOwner);
            }
        }

        // Store the result in the resolver context for the resolver to use
        resolverContext.SetScopedState("BlogOwnerNotesAccess", hasAccess);
        
        // Always succeed so the field can be queried
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}