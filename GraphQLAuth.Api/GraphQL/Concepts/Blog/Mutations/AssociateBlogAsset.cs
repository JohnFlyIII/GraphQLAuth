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
public class AssociateBlogAsset
{
    public async Task<Models.Blog> AssociateAssetToBlog(
        Guid blogId,
        Guid assetId,
        [Service] AppDbContext context,
        [Service] ClientResourceAuthorizer<Models.Blog> blogsAuthorizer,
        [Service] ClientResourceAuthorizer<Models.Asset> assetAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<AssociateBlogAsset> logger)
    {
        logger.LogInformation("Associating asset {AssetId} with blog {BlogId}", assetId, blogId);

        // Load blog and asset with authorization
        var blogQuery = context.Blogs.Where(b => b.Id == blogId);
        var blog = blogsAuthorizer.AuthorizeFilter(blogQuery).FirstOrDefault();
        
        if (blog == null)
        {
            logger.LogWarning("User {UserId} attempted to access unauthorized blog {BlogId}", 
                claimsPrincipal.Identity?.Name, blogId);
            throw new UnauthorizedAccessException("Blog not found or access denied");
        }

        var assetQuery = context.Assets.Where(a => a.Id == assetId);
        var asset = assetAuthorizer.AuthorizeFilter(assetQuery).FirstOrDefault();
        
        if (asset == null)
        {
            logger.LogWarning("User {UserId} attempted to access unauthorized asset {AssetId}", 
                claimsPrincipal.Identity?.Name, assetId);
            throw new UnauthorizedAccessException("Asset not found or access denied");
        }

        // Verify both belong to the same client (additional security check)
        if (blog.ClientId != asset.ClientId)
        {
            logger.LogWarning("User {UserId} attempted to associate asset {AssetId} from client {AssetClientId} with blog {BlogId} from client {BlogClientId}", 
                claimsPrincipal.Identity?.Name, assetId, asset.ClientId, blogId, blog.ClientId);
            throw new ArgumentException("Cannot associate assets across different clients");
        }

        // Load the blog with assets to check if already associated
        await context.Entry(blog)
            .Collection(b => b.Assets)
            .LoadAsync();

        // Check if already associated
        if (blog.Assets.Any(a => a.Id == assetId))
        {
            logger.LogInformation("Asset {AssetId} is already associated with blog {BlogId}", assetId, blogId);
            return blog;
        }

        // Add the association
        blog.Assets.Add(asset);
        blog.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Successfully associated asset {AssetId} with blog {BlogId}", assetId, blogId);
        return blog;
    }

    public async Task<Models.Blog> DisassociateAssetFromBlog(
        Guid blogId,
        Guid assetId,
        [Service] AppDbContext context,
        [Service] ClientResourceAuthorizer<Models.Blog> blogsAuthorizer,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<AssociateBlogAsset> logger)
    {
        logger.LogInformation("Disassociating asset {AssetId} from blog {BlogId}", assetId, blogId);

        // Load blog with authorization
        var blogQuery = context.Blogs.Where(b => b.Id == blogId);
        var blog = blogsAuthorizer.AuthorizeFilter(blogQuery).FirstOrDefault();
        
        if (blog == null)
        {
            logger.LogWarning("User {UserId} attempted to access unauthorized blog {BlogId}", 
                claimsPrincipal.Identity?.Name, blogId);
            throw new UnauthorizedAccessException("Blog not found or access denied");
        }

        // Load the blog's assets
        await context.Entry(blog)
            .Collection(b => b.Assets)
            .LoadAsync();

        // Find and remove the association
        var assetToRemove = blog.Assets.FirstOrDefault(a => a.Id == assetId);
        if (assetToRemove != null)
        {
            blog.Assets.Remove(assetToRemove);
            blog.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully disassociated asset {AssetId} from blog {BlogId}", assetId, blogId);
        }
        else
        {
            logger.LogInformation("Asset {AssetId} was not associated with blog {BlogId}", assetId, blogId);
        }

        return blog;
    }
}