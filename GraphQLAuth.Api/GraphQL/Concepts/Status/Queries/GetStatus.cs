using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Status.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
[Authorize(Policy = AuthConstants.Policies.RequireAnyRole)]
public class GetStatus
{
    public Models.Status? Status(
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] ILogger<GetStatus> logger)
    {
        var context = contextFactory.CreateDbContext();
        logger.LogInformation("GetStatus query executed");
        return context.Status.FirstOrDefault(s => s.Id == 1);
    }
}