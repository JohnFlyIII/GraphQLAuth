using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Authorization;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.Auth;

namespace GraphQLAuth.Api.GraphQL.Concepts.Asset.Types;

public class AssetType : ObjectType<Models.Asset>
{
    protected override void Configure(IObjectTypeDescriptor<Models.Asset> descriptor)
    {
        descriptor.Description("Represents an asset (image or audio) in the system");
        
        // Client access control is handled at query level via AssetAuthorizer filtering
        // No type-level authorization needed
        
        descriptor.BindFieldsExplicitly();
        
        descriptor.Field(a => a.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier of the asset");
            
        descriptor.Field(a => a.ClientId)
            .Type<NonNullType<IdType>>()
            .Description("The client this asset belongs to");
            
        descriptor.Field(a => a.Name)
            .Type<NonNullType<StringType>>()
            .Description("The name of the asset");
            
        descriptor.Field(a => a.Type)
            .Type<NonNullType<EnumType<Models.AssetType>>>()
            .Description("The type of asset (Image or Audio)");
            
        descriptor.Field(a => a.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the asset was created");
            
        descriptor.Field(a => a.UpdatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the asset was last updated");

        // Restricted field - only ClientOwners can access the actual asset data
        descriptor.Field(a => a.Base64Data)
            .Type<StringType>()
            .Description("Base64 encoded asset data (ClientOwner only)")
            .Authorize(AuthConstants.Policies.RequireAssetDataAccess);

        // Restricted field - only ClientOwners can access the actual asset data
        descriptor.Field(a => a.Url)
            .Type<StringType>()
            .Description("URL to the asset (ClientOwner only)")
            .Authorize(AuthConstants.Policies.RequireAssetDataAccess);

        // Navigation property for Blogs (many-to-many)
        descriptor.Field(a => a.Blogs)
            .Description("Blogs that this asset is associated with");
            
    }
}