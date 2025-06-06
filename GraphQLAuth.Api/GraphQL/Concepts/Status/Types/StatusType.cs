using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Authorization;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.Auth;

namespace GraphQLAuth.Api.GraphQL.Concepts.Status.Types;

public class StatusType : ObjectType<Models.Status>
{
    protected override void Configure(IObjectTypeDescriptor<Models.Status> descriptor)
    {
        descriptor.Description("Represents the system status information");

        descriptor.Field(s => s.Id)
            .Description("The status record identifier");
            
        descriptor.Field(s => s.Health)
            .Description("Current system health status");
            
        descriptor.Field(s => s.LastUpdated)
            .Description("When the status was last updated");
            
        descriptor.Field(s => s.SupportContact)
            .Description("Support contact information");
            
        descriptor.Field(s => s.SystemVersion)
            .Description("Current system version");

        // Admin-only field
        descriptor.Field(s => s.Notes)
            .Description("Internal admin notes (SystemAdmin only)")
            .Authorize(AuthConstants.Policies.RequireSystemAdmin);
    }
}