using System.Security.Claims;
using System.Linq.Expressions;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.GraphQL.Authorization;
using Microsoft.Extensions.Logging;

namespace GraphQLAuth.Api.GraphQL.Blogs;

public class BlogsAuthorizer : BaseClientAuthorizer<Blog>
{
    public BlogsAuthorizer(
        IHttpContextAccessor httpContextAccessor, 
        IAuthorizationService authService,
        ILogger<BaseClientAuthorizer<Blog>> logger) 
        : base(httpContextAccessor, authService, logger)
    {
    }

    protected override Expression<Func<Blog, Guid>> GetClientIdExpression()
    {
        return blog => blog.ClientId;
    }

    /// <summary>
    /// Check if user can view blog owner notes (ClientOwner role required)
    /// </summary>
    public bool CanViewBlogOwnerNotes(ClaimsPrincipal user, Blog blog)
    {
        if (!user.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("Unauthenticated user attempting to access blog owner notes for blog {BlogId}", blog.Id);
            return false;
        }

        // System admin can see all notes
        if (_authService.IsSystemAdmin(user))
        {
            _logger.LogDebug("SystemAdmin {UserId} accessing blog owner notes for blog {BlogId}", 
                user.Identity.Name, blog.Id);
            return true;
        }

        // Must have access to the blog AND be a ClientOwner
        if (!CanAccess(user, blog))
        {
            _logger.LogWarning("User {UserId} denied access to blog {BlogId} - no client access", 
                user.Identity.Name, blog.Id);
            return false;
        }

        var hasOwnerRole = _authService.HasClientRole(user, blog.ClientId, AuthConstants.Roles.ClientOwner);
        
        _logger.LogDebug("User {UserId} {Access} access to blog owner notes for blog {BlogId} (ClientId: {ClientId})", 
            user.Identity.Name, hasOwnerRole ? "granted" : "denied", blog.Id, blog.ClientId);

        // Only client owners can see notes
        return hasOwnerRole;
    }
}