using System.Security.Claims;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Blog.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[Authorize(Policy = AuthConstants.Policies.RequireClientTenant)]
public class CreateBlog
{
    public async Task<Models.Blog> CreateBlogPost(
        CreateBlogInput input,
        [Service] AppDbContext context,
        [Service] ClientResourceAuthorizer<Models.Blog> blogsAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<CreateBlog> logger)
    {
        logger.LogInformation("Creating blog with title {Title} for client {ClientId}", input.Title, input.ClientId);

        // Verify user has access to the specified client
        if (!authService.IsSystemAdmin(claimsPrincipal))
        {
            var hasClientAccess = authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientOwner) ||
                                 authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientUser);
            
            if (!hasClientAccess)
            {
                logger.LogWarning("User {UserId} attempted to create blog for unauthorized client {ClientId}", 
                    claimsPrincipal.Identity?.Name, input.ClientId);
                throw new UnauthorizedAccessException("You don't have access to this client");
            }
        }

        var blog = new Models.Blog
        {
            Id = Guid.NewGuid(),
            ClientId = input.ClientId,
            Title = input.Title,
            Content = input.Content,
            Author = input.Author,
            Summary = input.Summary,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tags = input.Tags ?? Array.Empty<string>()
        };

        context.Blogs.Add(blog);
        await context.SaveChangesAsync();

        logger.LogInformation("Blog {BlogId} created successfully for client {ClientId}", blog.Id, input.ClientId);
        return blog;
    }
}

public record CreateBlogInput(
    Guid ClientId,
    string Title,
    string Content,
    string Author,
    string? Summary = null,
    string[]? Tags = null);