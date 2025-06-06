using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Authorization;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.Auth;

namespace GraphQLAuth.Api.GraphQL.Concepts.Blog.Types;

public class BlogType : ObjectType<Models.Blog>
{
    protected override void Configure(IObjectTypeDescriptor<Models.Blog> descriptor)
    {
        descriptor.Description("Represents a blog post in the system");

        // Client access control is handled at query level via BlogsAuthorizer filtering
        // No type-level authorization needed

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
            .Authorize(AuthConstants.Policies.RequireClientOwnerRole)
            .Resolve(context =>
            {
                var blog = context.Parent<Models.Blog>();
                
                // Check the authorization result stored by the handler
                var hasAccess = context.GetScopedState<bool?>("ClientOwnerRoleAccess") ?? false;
                
                return hasAccess ? blog.BlogOwnerNotes : null;
            });

        // Navigation properties
        descriptor.Field(b => b.Client)
            .Description("The client organization this blog belongs to");
            
        descriptor.Field(b => b.Assets)
            .Description("Assets used in this blog post");
    }
}