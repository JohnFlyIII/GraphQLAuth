using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.Auth;
using System.Security.Claims;

namespace GraphQLAuth.Api.GraphQL.Types;

public class BlogType : ObjectType<Blog>
{
    protected override void Configure(IObjectTypeDescriptor<Blog> descriptor)
    {
        descriptor.Description("Represents a blog post in the system");

        descriptor.Field(b => b.Id)
            .Description("The unique identifier of the blog");
            
        descriptor.Field(b => b.ClientId)
            .Description("The client this blog belongs to");
            
        descriptor.Field(b => b.Title)
            .Description("The title of the blog post");
            
        descriptor.Field(b => b.Content)
            .Description("The main content of the blog post");
            
        descriptor.Field(b => b.Author)
            .Description("The author of the blog post");
            
        descriptor.Field(b => b.Summary)
            .Description("A brief summary of the blog post");
            
        descriptor.Field(b => b.IsPublished)
            .Description("Whether the blog post is published");
            
        descriptor.Field(b => b.CreatedAt)
            .Description("When the blog post was created");
            
        descriptor.Field(b => b.UpdatedAt)
            .Description("When the blog post was last updated");
            
        descriptor.Field(b => b.PublishedAt)
            .Description("When the blog post was published");
            
        descriptor.Field(b => b.Tags)
            .Description("Tags associated with the blog post");

        // Field-level authorization for BlogOwnerNotes
        descriptor.Field(b => b.BlogOwnerNotes)
            .Description("Private notes visible only to blog owners and system admins")
            .Resolve(context =>
            {
                var blog = context.Parent<Blog>();
                var user = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
                var authService = context.Service<IAuthorizationService>();

                // Only show notes to system admins or client owners
                if (authService.IsSystemAdmin(user) || 
                    authService.HasClientRole(user, blog.ClientId, AuthConstants.Roles.ClientOwner))
                {
                    return blog.BlogOwnerNotes;
                }

                return null;
            });
    }
}