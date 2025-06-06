namespace GraphQLAuth.Api.Auth;

/// <summary>
/// Interface for resources that belong to a specific client
/// Used for client-aware authorization
/// </summary>
public interface IClientResource
{
    /// <summary>
    /// The client ID that owns this resource
    /// </summary>
    Guid ClientId { get; }
}