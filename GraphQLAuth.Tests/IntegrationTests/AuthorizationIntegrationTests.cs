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
        dbContext.Blogs.RemoveRange(dbContext.Blogs);
        await dbContext.SaveChangesAsync();
        
        // Add test data
        var blogs = TestDataFactory.CreateTestBlogs();
        dbContext.Blogs.AddRange(blogs);
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
            TestDataFactory.Clients.AcmeCorp,
            TestDataFactory.Clients.TechStartup,
            TestDataFactory.Clients.MediaCompany
        });
    }

    [Fact]
    public async Task GetBlogs_AsClientOwner_ReturnsOnlyClientBlogsWithOwnerNotes()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateClientOwnerToken(TestDataFactory.Clients.AcmeCorp);
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
        response.Data.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.AcmeCorp);
        response.Data.Blogs.Should().OnlyContain(b => b.BlogOwnerNotes != null);
    }

    [Fact]
    public async Task GetBlogs_AsClientUser_ReturnsOnlyClientBlogsWithoutOwnerNotes()
    {
        // Arrange
        await SeedDatabase();
        var token = JwtTokenGenerator.GenerateClientUserToken(TestDataFactory.Clients.TechStartup);
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
        response.Data.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.TechStartup);
        response.Data.Blogs.Should().OnlyContain(b => b.BlogOwnerNotes == null); // No owner notes for ClientUser
    }

    [Fact]
    public async Task GetBlogs_WithMultipleClientRoles_ReturnsCorrectBlogs()
    {
        // Arrange
        await SeedDatabase();
        var roles = new[]
        {
            new ClientRole(TestDataFactory.Clients.AcmeCorp, AuthConstants.Roles.ClientOwner),
            new ClientRole(TestDataFactory.Clients.TechStartup, AuthConstants.Roles.ClientUser)
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
        response.Data!.Blogs.Should().HaveCount(4); // 2 from AcmeCorp + 2 from TechStartup
        
        // AcmeCorp blogs should have owner notes (ClientOwner role)
        var acmeBlogs = response.Data.Blogs.Where(b => b.ClientId == TestDataFactory.Clients.AcmeCorp);
        acmeBlogs.Should().HaveCount(2);
        acmeBlogs.Should().OnlyContain(b => b.BlogOwnerNotes != null);
        
        // TechStartup blogs should NOT have owner notes (ClientUser role)
        var techBlogs = response.Data.Blogs.Where(b => b.ClientId == TestDataFactory.Clients.TechStartup);
        techBlogs.Should().HaveCount(2);
        techBlogs.Should().OnlyContain(b => b.BlogOwnerNotes == null);
    }

    [Fact]
    public async Task GetBlog_AsClientOwner_CannotAccessOtherClientBlog()
    {
        // Arrange
        await SeedDatabase();
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediaCompanyBlog = dbContext.Blogs.First(b => b.ClientId == TestDataFactory.Clients.MediaCompany);
        
        var token = JwtTokenGenerator.GenerateClientOwnerToken(TestDataFactory.Clients.AcmeCorp);
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
        var response = await _client.QueryAsync<BlogResponse>(query, new { id = mediaCompanyBlog.Id });

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
        var token = JwtTokenGenerator.GenerateClientUserToken(TestDataFactory.Clients.TechStartup);
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
        response.Data!.Blogs.Should().OnlyContain(b => b.ClientId == TestDataFactory.Clients.TechStartup);
        response.Data.Blogs.Should().OnlyContain(b => b.IsPublished == true);
    }
}