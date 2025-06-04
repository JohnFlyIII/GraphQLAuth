using System;
using System.Linq;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using Microsoft.EntityFrameworkCore;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.GraphQL;

[Authorize]  // Require authentication for all queries
public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Blog> GetBlogs(
        [Service] AppDbContext context,
        [Service] IAuthorizationService authService,
        IResolverContext resolverContext)
    {
        var user = resolverContext.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
        
        if (authService.IsSystemAdmin(user))
        {
            return context.Blogs;
        }

        var clientRoles = authService.GetClientRoles(user);
        var allowedClientIds = clientRoles
            .Where(cr => cr.RoleId == AuthConstants.Roles.ClientOwner || cr.RoleId == AuthConstants.Roles.ClientUser)
            .Select(cr => cr.ClientId)
            .Distinct()
            .ToList();

        return context.Blogs.Where(b => allowedClientIds.Contains(b.ClientId));
    }

    public async Task<Blog?> GetBlog(
        Guid id,
        [Service] AppDbContext context,
        [Service] IAuthorizationService authService,
        IResolverContext resolverContext)
    {
        var user = resolverContext.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
        var blog = await context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
        
        if (blog == null)
        {
            return null;
        }

        if (authService.IsSystemAdmin(user))
        {
            return blog;
        }

        var hasAccess = authService.HasClientRole(user, blog.ClientId, AuthConstants.Roles.ClientOwner) ||
                       authService.HasClientRole(user, blog.ClientId, AuthConstants.Roles.ClientUser);

        return hasAccess ? blog : null;
    }
}