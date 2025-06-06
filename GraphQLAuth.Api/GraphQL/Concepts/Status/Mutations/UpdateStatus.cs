using GraphQLAuth.Api.Auth;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api.GraphQL.Concepts.Status.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
[Authorize(Policy = AuthConstants.Policies.RequireSystemAdmin)]
public class UpdateStatus
{
    public async Task<Models.Status> UpdateSystemStatus(
        UpdateStatusInput input,
        [Service] AppDbContext context,
        [Service] ILogger<UpdateStatus> logger)
    {
        logger.LogInformation("Updating system status");

        var status = await context.Status.FirstOrDefaultAsync(s => s.Id == 1);
        if (status == null)
        {
            // Create status if it doesn't exist
            status = new Models.Status { Id = 1 };
            context.Status.Add(status);
        }

        // Update fields
        status.Health = input.Health;
        status.LastUpdated = DateTimeOffset.UtcNow;
        status.SupportContact = input.SupportContact;
        status.SystemVersion = input.SystemVersion;
        if (input.Notes != null)
        {
            status.Notes = input.Notes;
        }

        await context.SaveChangesAsync();

        logger.LogInformation("System status updated successfully");
        return status;
    }
}

public record UpdateStatusInput(
    HealthStatus Health,
    string SupportContact,
    string SystemVersion,
    string? Notes = null);