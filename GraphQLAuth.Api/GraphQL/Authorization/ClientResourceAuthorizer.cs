using System.Security.Claims;
using System.Linq.Expressions;
using GraphQLAuth.Api.Auth;
using Microsoft.Extensions.Logging;

namespace GraphQLAuth.Api.GraphQL.Authorization;

/// <summary>
/// Generic authorizer for any entity that implements IClientResource
/// Eliminates the need for entity-specific authorizer classes
/// </summary>
public class ClientResourceAuthorizer<T> : BaseClientAuthorizer<T> where T : class, IClientResource
{
    public ClientResourceAuthorizer(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authService,
        ILogger<BaseClientAuthorizer<T>> logger)
        : base(httpContextAccessor, authService, logger)
    {
    }

    protected override IQueryable<T> AuthorizeQuery(IQueryable<T> query, IEnumerable<Guid> allowedIds)
    {
        // Uses the IClientResource interface - works for any implementing type
        return query.Where(x => allowedIds.Contains(x.ClientId));
    }
}