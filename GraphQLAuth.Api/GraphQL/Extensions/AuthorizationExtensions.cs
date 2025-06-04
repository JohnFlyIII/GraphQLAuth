using HotChocolate.Resolvers;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Models;
using System.Security.Claims;

namespace GraphQLAuth.Api.GraphQL.Extensions;

public static class AuthorizationExtensions
{
    public static bool CanAccessBlog(this IResolverContext context, Blog blog)
    {
        var claimsPrincipal = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
        if (claimsPrincipal == null || !claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        var authService = context.Service<IAuthorizationService>();
        
        // System admin can access everything
        if (authService.IsSystemAdmin(claimsPrincipal))
        {
            return true;
        }

        // Check if user has any role for this client
        return authService.HasClientRole(claimsPrincipal, blog.ClientId, AuthConstants.Roles.ClientOwner) ||
               authService.HasClientRole(claimsPrincipal, blog.ClientId, AuthConstants.Roles.ClientUser);
    }

    public static bool CanViewBlogOwnerNotes(this IResolverContext context, Blog blog)
    {
        var claimsPrincipal = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
        if (claimsPrincipal == null || !claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        var authService = context.Service<IAuthorizationService>();
        
        // System admin can see all notes
        if (authService.IsSystemAdmin(claimsPrincipal))
        {
            return true;
        }

        // Only client owners can see notes
        return authService.HasClientRole(claimsPrincipal, blog.ClientId, AuthConstants.Roles.ClientOwner);
    }
}