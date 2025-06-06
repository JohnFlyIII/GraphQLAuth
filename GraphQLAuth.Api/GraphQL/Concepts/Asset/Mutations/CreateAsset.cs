using System.Security.Claims;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Asset.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[Authorize(Policy = AuthConstants.Policies.RequireClientTenant)]
public class CreateAsset
{
    public async Task<Models.Asset> CreateAssetItem(
        CreateAssetInput input,
        [Service] AppDbContext context,
        [Service] ClientResourceAuthorizer<Models.Asset> assetAuthorizer,
        [Service] IAuthorizationService authService,
        ClaimsPrincipal claimsPrincipal,
        [Service] ILogger<CreateAsset> logger)
    {
        logger.LogInformation("Creating asset with name {Name} for client {ClientId}", input.Name, input.ClientId);

        // Verify user has access to the specified client
        if (!authService.IsSystemAdmin(claimsPrincipal))
        {
            var hasClientAccess = authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientOwner) ||
                                 authService.HasClientRole(claimsPrincipal, input.ClientId, AuthConstants.Roles.ClientUser);
            
            if (!hasClientAccess)
            {
                logger.LogWarning("User {UserId} attempted to create asset for unauthorized client {ClientId}", 
                    claimsPrincipal.Identity?.Name, input.ClientId);
                throw new UnauthorizedAccessException("You don't have access to this client");
            }
        }

        // Validate that only one of Base64Data or Url is provided
        if (!string.IsNullOrEmpty(input.Base64Data) && !string.IsNullOrEmpty(input.Url))
        {
            throw new ArgumentException("Cannot specify both Base64Data and Url");
        }

        if (string.IsNullOrEmpty(input.Base64Data) && string.IsNullOrEmpty(input.Url))
        {
            throw new ArgumentException("Must specify either Base64Data or Url");
        }

        var asset = new Models.Asset
        {
            Id = Guid.NewGuid(),
            ClientId = input.ClientId,
            Name = input.Name,
            Type = input.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Use the domain logic methods to set data
        if (!string.IsNullOrEmpty(input.Base64Data))
        {
            asset.SetBase64Data(input.Base64Data);
        }
        else
        {
            asset.SetUrl(input.Url!);
        }

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        logger.LogInformation("Asset {AssetId} created successfully for client {ClientId}", asset.Id, input.ClientId);
        return asset;
    }
}

public record CreateAssetInput(
    Guid ClientId,
    string Name,
    AssetType Type,
    string? Base64Data = null,
    string? Url = null);