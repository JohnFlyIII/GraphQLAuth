using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Auth;
using GraphQLAuth.Tests.DTOs;
using GraphQLAuth.Tests.Infrastructure;

namespace GraphQLAuth.Tests.IntegrationTests;

public class AuthorizationIntegrationTests : IClassFixture<GraphQLAuthWebApplicationFactory>
{
    private readonly GraphQLAuthWebApplicationFactory _factory;
    private readonly GraphQLTestClient _client;

    public AuthorizationIntegrationTests(GraphQLAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = new GraphQLTestClient(_factory.CreateClient());
    }

    private async Task SeedDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Clear existing data
        dbContext.Assets.RemoveRange(dbContext.Assets);
        dbContext.Blogs.RemoveRange(dbContext.Blogs);
        dbContext.Clients.RemoveRange(dbContext.Clients);
        await dbContext.SaveChangesAsync();
        
        // Add test data
        var clients = TestDataFactory.CreateTestClients();
        dbContext.Clients.AddRange(clients);
        
        var blogs = TestDataFactory.CreateTestBlogs();
        dbContext.Blogs.AddRange(blogs);
        
        var assets = TestDataFactory.CreateTestAssets();
        dbContext.Assets.AddRange(assets);
        
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetBlogs_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await SeedDatabase();
        _client.ClearAuthorizationToken();
        
        var query = @"
            query {
                blogs {
                    id
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().NotBeNullOrEmpty();
        response.Errors![0].Message.Should().Contain("not authorized");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetBlogs_AsSystemAdmin_ReturnsAllBlogsWithOwnerNotes()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateSystemAdminToken();
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query {
                blogs {
                    id
                    clientId
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blogs.Should().HaveCount(6); // All blogs from all clients
        response.Data.Blogs.Should().OnlyContain(b => b.BlogOwnerNotes != null);
        
        // Verify we have blogs from all three clients
        var clientIds = response.Data.Blogs.Select(b => b.ClientId).Distinct();
        clientIds.Should().BeEquivalentTo(new[] 
        { 
            TestDataFactory.Clients.LizProBloggers,
            TestDataFactory.Clients.DavidsDiscountContent,
            TestDataFactory.Clients.PaulsPremiumBlogs
        });
    }

    [Fact]
    public async Task GetBlogs_AsClientOwner_ReturnsOnlyClientBlogsWithOwnerNotes()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateClientOwnerToken(TestDataFactory.Clients.LizProBloggers);
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query {
                blogs {
                    id
                    clientId
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blogs.Should().HaveCount(2); // Only AcmeCorp blogs
        response.Data.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.LizProBloggers);
        response.Data.Blogs.Should().OnlyContain(b => b.BlogOwnerNotes != null);
    }

    [Fact]
    public async Task GetBlogs_AsClientUser_ReturnsOnlyClientBlogsWithoutOwnerNotes()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateClientUserToken(TestDataFactory.Clients.DavidsDiscountContent);
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query {
                blogs {
                    id
                    clientId
                    title
                    author
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blogs.Should().HaveCount(2); // Only TechStartup blogs
        response.Data.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.DavidsDiscountContent);
        response.Data.Blogs.Should().OnlyContain(b => b.BlogOwnerNotes == null); // No owner notes for ClientUser
    }

    [Fact]
    public async Task GetBlogs_WithMultipleClientRoles_ReturnsCorrectBlogs()
    {
        // Arrange
        await SeedDatabase();
        var roles = new[]
        {
            new ClientRole(TestDataFactory.Clients.LizProBloggers, AuthConstants.Roles.ClientOwner),
            new ClientRole(TestDataFactory.Clients.DavidsDiscountContent, AuthConstants.Roles.ClientUser)
        };
        var token = JwtTokenGenerator.GenerateMultiClientToken(roles);
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query {
                blogs {
                    id
                    clientId
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blogs.Should().HaveCount(4); // 2 from LizProBloggers + 2 from DavidsDiscountContent
        
        // LizProBloggers blogs should have owner notes (ClientOwner role)
        var lizBlogs = response.Data.Blogs.Where(b => b.ClientId == TestDataFactory.Clients.LizProBloggers);
        lizBlogs.Should().HaveCount(2);
        lizBlogs.Should().OnlyContain(b => b.BlogOwnerNotes != null);
        
        // DavidsDiscountContent blogs should NOT have owner notes (ClientUser role)
        var davidBlogs = response.Data.Blogs.Where(b => b.ClientId == TestDataFactory.Clients.DavidsDiscountContent);
        davidBlogs.Should().HaveCount(2);
        davidBlogs.Should().OnlyContain(b => b.BlogOwnerNotes == null);
    }

    [Fact]
    public async Task GetBlog_AsClientOwner_CannotAccessOtherClientBlog()
    {
        // Arrange
        await SeedDatabase();
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paulsBlog = dbContext.Blogs.First(b => b.ClientId == TestDataFactory.Clients.PaulsPremiumBlogs);
        
        var token = JwtTokenGenerator.GenerateClientOwnerToken(TestDataFactory.Clients.LizProBloggers);
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query($id: UUID!) {
                blog(id: $id) {
                    id
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogResponse>(query, new { id = paulsBlog.Id });

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blog.Should().BeNull(); // No access to other client's blog
    }

    [Fact]
    public async Task GetBlog_AsSystemAdmin_CanAccessAnyBlog()
    {
        // Arrange
        await SeedDatabase();
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var randomBlog = dbContext.Blogs.First();
        
        var token = JwtTokenGenerator.GenerateSystemAdminToken();
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query($id: UUID!) {
                blog(id: $id) {
                    id
                    title
                    blogOwnerNotes
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogResponse>(query, new { id = randomBlog.Id });

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blog.Should().NotBeNull();
        response.Data.Blog!.Id.Should().Be(randomBlog.Id);
        response.Data.Blog.BlogOwnerNotes.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBlogs_WithFiltering_RespectsAuthorizationScope()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateClientUserToken(TestDataFactory.Clients.DavidsDiscountContent);
        _client.SetAuthorizationToken(token);
        
        var query = @"
            query {
                blogs(where: { isPublished: { eq: true } }) {
                    id
                    clientId
                    title
                    isPublished
                }
            }";

        // Act
        var response = await _client.QueryAsync<BlogsResponse>(query);

        // Assert
        response.Errors.Should().BeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.DavidsDiscountContent);
        response.Data.Blogs.Should().OnlyContain(b => b.IsPublished == true);
    }
}