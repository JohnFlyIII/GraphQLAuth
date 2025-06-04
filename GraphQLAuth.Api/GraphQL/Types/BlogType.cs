using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using HotChocolate.Authorization;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.Auth;
using System.Security.Claims;

namespace GraphQLAuth.Api.GraphQL.Types;

public class BlogType : ObjectType<Blog>
{
    protected override void Configure(IObjectTypeDescriptor<Blog> descriptor)
    {
        descriptor.Description("Represents a blog post in the system");

        // Apply authorization to the entire type - users must be authenticated
        descriptor.Authorize();

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

        // Field-level authorization using HotChocolate policy with context-aware handler
        descriptor.Field(b => b.BlogOwnerNotes)
            .Description("Private notes visible only to blog owners and system admins")
            .Authorize(AuthConstants.Policies.RequireBlogOwnerNotesAccess)
            .Resolve(context =>
            {
                var blog = context.Parent<Blog>();
                
                // Check the authorization result stored by the handler
                var hasAccess = context.GetScopedState<bool?>("BlogOwnerNotesAccess") ?? false;
                
                return hasAccess ? blog.BlogOwnerNotes : null;
            });
    }
}