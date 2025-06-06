using GraphQLAuth.Api.Auth;
using HotChocolate.Authorization;
using Microsoft.Extensions.Logging;

namespace GraphQLAuth.Api.GraphQL.Concepts.Auth.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GenerateToken
{
    [AllowAnonymous]
    public string GenerateAuthToken(
        string role,
        Guid? clientId,
        [Service] TokenGenerator tokenGenerator,
        [Service] ILogger<GenerateToken> logger)
    {
        logger.LogInformation("Generating token for role {Role} and clientId {ClientId}", role, clientId);

        var clientRoles = new List<ClientRole>();

        switch (role.ToLower())
        {
            case "systemadmin":
                clientRoles.Add(new ClientRole(clientId ?? Guid.NewGuid(), AuthConstants.Roles.SystemAdmin));
                break;
            case "clientowner":
                if (!clientId.HasValue)
                    throw new ArgumentException("ClientId is required for ClientOwner role");
                clientRoles.Add(new ClientRole(clientId.Value, AuthConstants.Roles.ClientOwner));
                break;
            case "clientuser":
                if (!clientId.HasValue)
                    throw new ArgumentException("ClientId is required for ClientUser role");
                clientRoles.Add(new ClientRole(clientId.Value, AuthConstants.Roles.ClientUser));
                break;
            default:
                throw new ArgumentException($"Invalid role: {role}");
        }

        var token = tokenGenerator.GenerateToken(clientRoles.ToArray());
        
        logger.LogInformation("Token generated successfully for role {Role} and clientId {ClientId}", role, clientId);
        return token;
    }
}