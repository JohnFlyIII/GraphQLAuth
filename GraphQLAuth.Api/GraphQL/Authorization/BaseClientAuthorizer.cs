using System.Security.Claims;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Models;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace GraphQLAuth.Api.GraphQL.Authorization;

public abstract class BaseClientAuthorizer<T> where T : class
{
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IAuthorizationService _authService;
    protected readonly ILogger<BaseClientAuthorizer<T>> _logger;

    protected BaseClientAuthorizer(
        IHttpContextAccessor httpContextAccessor, 
        IAuthorizationService authService,
        ILogger<BaseClientAuthorizer<T>> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Must be implemented by each entity authorizer to specify how to get ClientId from the entity
    /// </summary>
    protected abstract Expression<Func<T, Guid>> GetClientIdExpression();

    /// <summary>
    /// Mandatory client filtering - CANNOT be bypassed except for SystemAdmin
    /// </summary>
    public IQueryable<T> AuthorizeFilter(IQueryable<T> query)
    {
        var user = GetCurrentUser();
        var entityType = typeof(T).Name;
        
        _logger.LogDebug("Applying client authorization filter for {EntityType}", entityType);
        
        // Only SystemAdmin can bypass client filtering
        if (_authService.IsSystemAdmin(user))
        {
            _logger.LogDebug("SystemAdmin detected, bypassing client filtering for {EntityType}", entityType);
            return query;
        }

        // Get allowed client IDs - this is MANDATORY for non-admins
        var allowedClientIds = GetAllowedClientIds(user);
        
        _logger.LogDebug("User has access to {ClientCount} clients for {EntityType}: {ClientIds}", 
            allowedClientIds.Count, entityType, allowedClientIds);
        
        if (!allowedClientIds.Any())
        {
            _logger.LogWarning("User {UserId} has no client access for {EntityType}, returning empty result", 
                user.Identity?.Name ?? "Unknown", entityType);
            return query.Where(_ => false);
        }

        // Apply client filtering using the expression
        var clientIdExpression = GetClientIdExpression();
        var parameter = clientIdExpression.Parameters[0];
        var body = clientIdExpression.Body;
        
        // Create expression: allowedClientIds.Contains(entity.ClientId)
        var containsMethod = typeof(List<Guid>).GetMethod("Contains", new[] { typeof(Guid) });
        if (containsMethod == null)
        {
            throw new InvalidOperationException("Could not find Contains method on List<Guid>");
        }
        
        var containsExpression = Expression.Call(
            Expression.Constant(allowedClientIds),
            containsMethod,
            body
        );
        
        var predicate = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        
        _logger.LogDebug("Applied client filtering for {EntityType} to {ClientCount} allowed clients", 
            entityType, allowedClientIds.Count);
        
        return query.Where(predicate);
    }

    protected virtual ClaimsPrincipal GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User ?? 
               throw new UnauthorizedAccessException("No authenticated user found");
    }

    protected virtual List<Guid> GetAllowedClientIds(ClaimsPrincipal user)
    {
        var clientRoles = _authService.GetClientRoles(user);
        return clientRoles
            .Where(cr => cr.RoleId == AuthConstants.Roles.ClientOwner || 
                        cr.RoleId == AuthConstants.Roles.ClientUser)
            .Select(cr => cr.ClientId)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Check if user can access a specific entity
    /// </summary>
    public virtual bool CanAccess(ClaimsPrincipal user, T entity)
    {
        if (!user.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        if (_authService.IsSystemAdmin(user))
        {
            return true;
        }

        // Get ClientId from entity using compiled expression
        var clientIdExpression = GetClientIdExpression();
        var compiledExpression = clientIdExpression.Compile();
        var entityClientId = compiledExpression(entity);

        var allowedClientIds = GetAllowedClientIds(user);
        return allowedClientIds.Contains(entityClientId);
    }
}