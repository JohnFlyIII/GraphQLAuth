using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.GraphQL.Authorization;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Asset.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
[Authorize(Policy = AuthConstants.Policies.RequireAnyRole)]
public class GetAssetLibrary
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Models.Asset> AssetLibrary(
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] ClientResourceAuthorizer<Models.Asset> assetAuthorizer,
        [Service] ILogger<GetAssetLibrary> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetAssetLibrary query executed");
        return assetAuthorizer.AuthorizeFilter(context.Assets);
    }
}