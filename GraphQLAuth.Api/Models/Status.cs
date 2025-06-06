using System;

namespace GraphQLAuth.Api.Models;

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public class Status
{
    public int Id { get; set; } = 1; // Singleton pattern - always use Id = 1
    public HealthStatus Health { get; set; } = HealthStatus.Healthy;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public string SupportContact { get; set; } = "support@demoapp.com";
    public string SystemVersion { get; set; } = "1.0.0";
    public string? Notes { get; set; } // Admin-only field for internal notes
}