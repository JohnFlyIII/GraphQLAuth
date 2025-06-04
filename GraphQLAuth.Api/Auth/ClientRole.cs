using System;

namespace GraphQLAuth.Api.Auth;

public record ClientRole(Guid ClientId, string RoleId);