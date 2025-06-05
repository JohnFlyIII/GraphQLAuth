using System.Security.Claims;
using System.Linq.Expressions;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Models;
using GraphQLAuth.Api.GraphQL.Authorization;
using Microsoft.Extensions.Logging;

namespace GraphQLAuth.Api.GraphQL.Assets;

public class AssetAuthorizer : BaseClientAuthorizer<Asset>
{
    public AssetAuthorizer(
        IHttpContextAccessor httpContextAccessor, 
        IAuthorizationService authService,
        ILogger<BaseClientAuthorizer<Asset>> logger) 
        : base(httpContextAccessor, authService, logger)
    {
    }

    protected override Expression<Func<Asset, Guid>> GetClientIdExpression()
    {
        return asset => asset.ClientId;
    }

    /// <summary>
    /// Check if user can view asset data (ClientOwner role required)
    /// </summary>
    public bool CanViewAssetData(ClaimsPrincipal user, Asset asset)
    {
        if (!user.Identity?.IsAuthenticated == true)
        {
            _logger.LogDebug("Unauthenticated user attempting to access asset data for asset {AssetId}", asset.Id);
            return false;
        }

        // System admin can see all asset data
        if (_authService.IsSystemAdmin(user))
        {
            _logger.LogDebug("SystemAdmin {UserId} accessing asset data for asset {AssetId}", 
                user.Identity.Name, asset.Id);
            return true;
        }

        // Must have access to the asset AND be a ClientOwner
        if (!CanAccess(user, asset))
        {
            _logger.LogWarning("User {UserId} denied access to asset {AssetId} - no client access", 
                user.Identity.Name, asset.Id);
            return false;
        }

        var hasOwnerRole = _authService.HasClientRole(user, asset.ClientId, AuthConstants.Roles.ClientOwner);
        
        _logger.LogDebug("User {UserId} {Access} access to asset data for asset {AssetId} (ClientId: {ClientId})", 
            user.Identity.Name, hasOwnerRole ? "granted" : "denied", asset.Id, asset.ClientId);

        // Only client owners can view full asset data (base64/URL)
        return hasOwnerRole;
    }
}