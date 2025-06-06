using HotChocolate.Authorization;

namespace GraphQLAuth.Api.GraphQL.Concepts.Auth.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetTestClientIds
{
    [AllowAnonymous]
    public TestClientIds TestClientIds()
    {
        return new TestClientIds(
            LizProBloggers: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            DavidsDiscountContent: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            PaulsPremiumBlogs: Guid.Parse("33333333-3333-3333-3333-333333333333")
        );
    }
}

public record TestClientIds(
    Guid LizProBloggers,
    Guid DavidsDiscountContent,
    Guid PaulsPremiumBlogs);