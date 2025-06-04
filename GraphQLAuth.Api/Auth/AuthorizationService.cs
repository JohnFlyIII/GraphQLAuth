using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace GraphQLAuth.Api.Auth;

public interface IAuthorizationService
{
    bool IsSystemAdmin(ClaimsPrincipal user);
    bool HasClientRole(ClaimsPrincipal user, Guid clientId, string roleId);
    IEnumerable<ClientRole> GetClientRoles(ClaimsPrincipal user);
}

public class AuthorizationService : IAuthorizationService
{
    public bool IsSystemAdmin(ClaimsPrincipal user)
    {
        var clientRoles = GetClientRoles(user);
        return clientRoles.Any(cr => cr.RoleId == AuthConstants.Roles.SystemAdmin);
    }

    public bool HasClientRole(ClaimsPrincipal user, Guid clientId, string roleId)
    {
        var clientRoles = GetClientRoles(user);
        return clientRoles.Any(cr => cr.ClientId == clientId && cr.RoleId == roleId);
    }

    public IEnumerable<ClientRole> GetClientRoles(ClaimsPrincipal user)
    {
        var clientRolesClaim = user.Claims.FirstOrDefault(c => c.Type == AuthConstants.ClientRolesClaim);
        if (clientRolesClaim == null || string.IsNullOrEmpty(clientRolesClaim.Value))
        {
            return Enumerable.Empty<ClientRole>();
        }

        try
        {
            var rolesData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(clientRolesClaim.Value);
            if (rolesData == null)
            {
                return Enumerable.Empty<ClientRole>();
            }

            var clientRoles = new List<ClientRole>();
            foreach (var roleData in rolesData)
            {
                if (roleData.TryGetValue("ClientId", out var clientIdStr) && 
                    roleData.TryGetValue("RoleId", out var roleId) &&
                    Guid.TryParse(clientIdStr, out var clientId))
                {
                    clientRoles.Add(new ClientRole(clientId, roleId));
                }
            }

            return clientRoles;
        }
        catch
        {
            return Enumerable.Empty<ClientRole>();
        }
    }
}