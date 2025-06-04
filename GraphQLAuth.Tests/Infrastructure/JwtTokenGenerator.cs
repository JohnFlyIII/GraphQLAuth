using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using GraphQLAuth.Api.Auth;

namespace GraphQLAuth.Tests.Infrastructure;

public static class JwtTokenGenerator
{
    private const string TestSecretKey = "SuperSecretKeyThatShouldBeStoredSecurely123!";
    private const string TestIssuer = "GraphQLAuth.Api";
    private const string TestAudience = "GraphQLAuth.Client";

    public static string GenerateToken(params ClientRole[] clientRoles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TestSecretKey);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Test User")
        };

        if (clientRoles.Any())
        {
            var rolesData = clientRoles.Select(cr => new Dictionary<string, string>
            {
                { "ClientId", cr.ClientId.ToString() },
                { "RoleId", cr.RoleId }
            }).ToList();

            claims.Add(new Claim(AuthConstants.ClientRolesClaim, JsonSerializer.Serialize(rolesData)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateSystemAdminToken(Guid? clientId = null)
    {
        var clientRoles = new List<ClientRole>
        {
            new ClientRole(clientId ?? Guid.NewGuid(), AuthConstants.Roles.SystemAdmin)
        };
        return GenerateToken(clientRoles.ToArray());
    }

    public static string GenerateClientOwnerToken(Guid clientId)
    {
        return GenerateToken(new ClientRole(clientId, AuthConstants.Roles.ClientOwner));
    }

    public static string GenerateClientUserToken(Guid clientId)
    {
        return GenerateToken(new ClientRole(clientId, AuthConstants.Roles.ClientUser));
    }

    public static string GenerateMultiClientToken(params ClientRole[] roles)
    {
        return GenerateToken(roles);
    }
}