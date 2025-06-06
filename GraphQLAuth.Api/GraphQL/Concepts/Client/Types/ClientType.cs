using HotChocolate;
using HotChocolate.Types;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.GraphQL.Concepts.Client.Types;

public class ClientType : ObjectType<Models.Client>
{
    protected override void Configure(IObjectTypeDescriptor<Models.Client> descriptor)
    {
        descriptor.Description("Represents a client organization in the system");

        descriptor.Field(c => c.ClientId)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier of the client");
            
        descriptor.Field(c => c.Name)
            .Type<NonNullType<StringType>>()
            .Description("The name of the client organization");
            
        descriptor.Field(c => c.Description)
            .Type<NonNullType<StringType>>()
            .Description("A description of the client organization");
    }
}