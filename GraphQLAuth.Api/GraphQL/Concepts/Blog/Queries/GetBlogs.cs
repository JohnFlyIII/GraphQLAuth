using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Blog.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
[Authorize(Policy = AuthConstants.Policies.RequireAnyRole)]
public class GetBlogs
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Models.Blog> Blogs(
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] ClientResourceAuthorizer<Models.Blog> blogsAuthorizer,
        [Service] ILogger<GetBlogs> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetBlogs query executed");
        return blogsAuthorizer.AuthorizeFilter(context.Blogs);
    }
}