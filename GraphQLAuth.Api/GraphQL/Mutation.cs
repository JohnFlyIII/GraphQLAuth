using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.Extensions.Logging;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.GraphQL.Blogs;
using GraphQLAuth.Api.GraphQL.Assets;

namespace GraphQLAuth.Api.GraphQL;

[Authorize(Policy = AuthConstants.Policies.RequireClientTenant)]
public class Mutation
{
    private readonly ILogger<Mutation> _logger;

    public Mutation(ILogger<Mutation> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    public async Task<string> GenerateToken(
        string role,
        Guid? clientId,
        [Service] TokenGenerator tokenGenerator,
        [Service] ILogger<Mutation> logger)
    {
        logger.LogInformation("Generating token for role {Role} and clientId {ClientId}", role, clientId);

        var clientRoles = new List<ClientRole>();

        switch (role.ToLower())
        {
            case "systemadmin":
                clientRoles.Add(new ClientRole(clientId ?? Guid.NewGuid(), AuthConstants.Roles.SystemAdmin));
                break;
            case "clientowner":
                if (!clientId.HasValue)
                    throw new ArgumentException("ClientId is required for ClientOwner role");
                clientRoles.Add(new ClientRole(clientId.Value, AuthConstants.Roles.ClientOwner));
                break;
            case "clientuser":
                if (!clientId.HasValue)
                    throw new ArgumentException("ClientId is required for ClientUser role");
                clientRoles.Add(new ClientRole(clientId.Value, AuthConstants.Roles.ClientUser));
                break;
            default:
                throw new ArgumentException($"Invalid role: {role}");
        }

        var token = tokenGenerator.GenerateToken(clientRoles.ToArray());
        
        logger.LogInformation("Token generated successfully for role {Role} and clientId {ClientId}", role, clientId);
        return token;
    }

    [AllowAnonymous]
    public TestClientIds GetTestClientIds()
    {
        return new TestClientIds(
            LizProBloggers: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            DavidsDiscountContent: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            PaulsPremiumBlogs: Guid.Parse("33333333-3333-3333-3333-333333333333")
        );
    }

    public async Task<Blog> CreateBlog(
        CreateBlogInput input,
        [Service] AppDbContext context,
        [Service] BlogsAuthorizer blogsAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<Mutation> logger)
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

        var blog = new Blog
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

    public async Task<Asset> CreateAsset(
        CreateAssetInput input,
        [Service] AppDbContext context,
        [Service] AssetAuthorizer assetAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<Mutation> logger)
    {
        logger.LogInformation("Creating asset with name {Name} for client {ClientId}", input.Name, input.ClientId);

        // Verify user has access to the specified client
        if (!authService.IsSystemAdmin(claimsPrincipal))
        {
            var hasClientAccess = authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientOwner) ||
                                 authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientUser);
            
            if (!hasClientAccess)
            {
                logger.LogWarning("User {UserId} attempted to create asset for unauthorized client {ClientId}", 
                    claimsPrincipal.Identity?.Name, input.ClientId);
                throw new UnauthorizedAccessException("You don't have access to this client");
            }
        }

        // Validate that only one of Base64Data or Url is provided
        if (!string.IsNullOrEmpty(input.Base64Data) && !string.IsNullOrEmpty(input.Url))
        {
            throw new ArgumentException("Cannot specify both Base64Data and Url");
        }

        if (string.IsNullOrEmpty(input.Base64Data) && string.IsNullOrEmpty(input.Url))
        {
            throw new ArgumentException("Must specify either Base64Data or Url");
        }

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = input.ClientId,
            Name = input.Name,
            Type = input.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Use the domain logic methods to set data
        if (!string.IsNullOrEmpty(input.Base64Data))
        {
            asset.SetBase64Data(input.Base64Data);
        }
        else
        {
            asset.SetUrl(input.Url!);
        }

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        logger.LogInformation("Asset {AssetId} created successfully for client {ClientId}", asset.Id, input.ClientId);
        return asset;
    }

    public async Task<Blog> AssociateAssetToBlog(
        Guid blogId,
        Guid assetId,
        [Service] AppDbContext context,
        [Service] BlogsAuthorizer blogsAuthorizer,
        [Service] AssetAuthorizer assetAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<Mutation> logger)
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

    public async Task<Blog> DisassociateAssetFromBlog(
        Guid blogId,
        Guid assetId,
        [Service] AppDbContext context,
        [Service] BlogsAuthorizer blogsAuthorizer,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<Mutation> logger)
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

// Input types for mutations
public record CreateBlogInput(
    Guid ClientId,
    string Title,
    string Content,
    string Author,
    string? Summary = null,
    string[]? Tags = null);

public record CreateAssetInput(
    Guid ClientId,
    string Name,
    AssetType Type,
    string? Base64Data = null,
    string? Url = null);

public record TestClientIds(
    Guid LizProBloggers,
    Guid DavidsDiscountContent,
    Guid PaulsPremiumBlogs);