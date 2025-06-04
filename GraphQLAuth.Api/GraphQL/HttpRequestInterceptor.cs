using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace GraphQLAuth.Api.GraphQL;

public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(HttpContext context,
        IRequestExecutor requestExecutor, OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        // Pass the ClaimsPrincipal to the GraphQL execution context
        requestBuilder.SetGlobalState("ClaimsPrincipal", context.User);
        
        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}