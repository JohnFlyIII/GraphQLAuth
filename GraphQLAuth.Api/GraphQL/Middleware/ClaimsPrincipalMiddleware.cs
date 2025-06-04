using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using System.Security.Claims;

namespace GraphQLAuth.Api.GraphQL.Middleware;

public class ClaimsPrincipalMiddleware
{
    private readonly HotChocolate.Execution.RequestDelegate _next;

    public ClaimsPrincipalMiddleware(HotChocolate.Execution.RequestDelegate next)
    {
        _next = next;
    }

    public async ValueTask InvokeAsync(IRequestContext context)
    {
        if (context.ContextData.TryGetValue("HttpContext", out var value) && 
            value is Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            context.ContextData["ClaimsPrincipal"] = httpContext.User;
        }

        await _next(context);
    }
}