using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Blog.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
[Authorize(Policy = AuthConstants.Policies.RequireAnyRole)]
public class GetBlog
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public Models.Blog? Blog(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] ClientResourceAuthorizer<Models.Blog> blogsAuthorizer,
        [Service] ILogger<GetBlog> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetBlog query executed for blog {BlogId}", id);
        var blogQueryable = context.Blogs.Where(b => b.Id == id);
        return blogsAuthorizer.AuthorizeFilter(blogQueryable).FirstOrDefault();
    }
}