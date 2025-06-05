using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.GraphQL.Assets;
using System.Security.Claims;

namespace GraphQLAuth.Api.GraphQL.Types;

public class AssetType : ObjectType<Asset>
{
    protected override void Configure(IObjectTypeDescriptor<Asset> descriptor)
    {
        descriptor.Description("Represents an asset (image or audio) in the system");
        
        // Apply authorization to the entire type - users must be authenticated
        descriptor.Authorize();
        
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
            .Resolve(context =>
            {
                var asset = context.Parent<Asset>();
                var claimsPrincipal = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
                var assetAuthorizer = context.Service<AssetAuthorizer>();
                
                if (claimsPrincipal != null && assetAuthorizer.CanViewAssetData(claimsPrincipal, asset))
                {
                    return asset.Base64Data;
                }
                
                return null; // Hide data from ClientUsers
            });

        // Restricted field - only ClientOwners can access the actual asset data
        descriptor.Field(a => a.Url)
            .Type<StringType>()
            .Description("URL to the asset (ClientOwner only)")
            .Resolve(context =>
            {
                var asset = context.Parent<Asset>();
                var claimsPrincipal = context.GetGlobalState<ClaimsPrincipal>("ClaimsPrincipal");
                var assetAuthorizer = context.Service<AssetAuthorizer>();
                
                if (claimsPrincipal != null && assetAuthorizer.CanViewAssetData(claimsPrincipal, asset))
                {
                    return asset.Url;
                }
                
                return null; // Hide data from ClientUsers
            });

        // Navigation property for Blogs (many-to-many)
        descriptor.Field(a => a.Blogs)
            .Description("Blogs that this asset is associated with");
            
    }
}