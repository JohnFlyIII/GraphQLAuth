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
        [Service] BlogsAuthorizer blogsAuthorizer)
    {
        Console.WriteLine("asdfadsfa");
        return blogsAuthorizer.AuthorizeFilter(context.Blogs);
    }

    public Blog? GetBlog(
        Guid id,
        [Service] AppDbContext context,
        [Service] BlogsAuthorizer blogsAuthorizer)
    {
        var blogQueryable = context.Blogs.Where(b => b.Id == id);
        return blogsAuthorizer.AuthorizeFilter(blogQueryable).FirstOrDefault();
    }

}