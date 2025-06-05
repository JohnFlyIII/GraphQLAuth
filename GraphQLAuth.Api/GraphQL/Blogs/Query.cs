using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Assets;
using GraphQLAuth.Api.GraphQL.Blogs;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

[Authorize(Policy = AuthConstants.Policies.RequireClientTenant)]
public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Blog> GetBlogs(
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] BlogsAuthorizer blogsAuthorizer,
        [Service] ILogger<Query> logger)
    {
        // Don't use 'using' here - Hot Chocolate will manage the context lifetime
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetBlogs query executed");
        return blogsAuthorizer.AuthorizeFilter(context.Blogs);
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public Blog? GetBlog(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] BlogsAuthorizer blogsAuthorizer,
        [Service] ILogger<Query> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetBlog query executed for blog {BlogId}", id);
        var blogQueryable = context.Blogs.Where(b => b.Id == id);
        return blogsAuthorizer.AuthorizeFilter(blogQueryable).FirstOrDefault();
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Asset> GetAssetLibrary(
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] AssetAuthorizer assetAuthorizer,
        [Service] ILogger<Query> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetAssetLibrary query executed");
        return assetAuthorizer.AuthorizeFilter(context.Assets);
    }
}