using System;

namespace GraphQLAuth.Api.Models;

public class Client
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}