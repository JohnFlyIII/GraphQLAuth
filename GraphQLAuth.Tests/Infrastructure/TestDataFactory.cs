using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Tests.Infrastructure;

public static class TestDataFactory
{
    public static class Clients
    {
        public static readonly Guid AcmeCorp = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TechStartup = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid MediaCompany = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }

    public static List<Blog> CreateTestBlogs()
    {
        var blogs = new List<Blog>();

        // AcmeCorp blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.AcmeCorp,
            Title = "AcmeCorp Q4 Results",
            Content = "Our Q4 results show strong growth across all divisions.",
            Author = "CEO John Smith",
            Summary = "Q4 financial results summary",
            BlogOwnerNotes = "Internal: Revenue exceeded expectations by 15%",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            PublishedAt = DateTime.UtcNow.AddDays(-5),
            Tags = new[] { "financial", "quarterly-results", "growth" }
        });

        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.AcmeCorp,
            Title = "New Product Launch",
            Content = "We're excited to announce our new product line.",
            Author = "Product Manager",
            Summary = "Introducing our innovative new products",
            BlogOwnerNotes = "Marketing team: Focus on B2B segment for initial launch",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Tags = new[] { "product", "innovation", "announcement" }
        });

        // TechStartup blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.TechStartup,
            Title = "AI Integration Success Story",
            Content = "How we integrated AI into our workflow and improved efficiency by 40%.",
            Author = "CTO Sarah Johnson",
            Summary = "Our journey with AI integration",
            BlogOwnerNotes = "Tech details: Using GPT-4 API with custom fine-tuning",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            UpdatedAt = DateTime.UtcNow.AddDays(-12),
            PublishedAt = DateTime.UtcNow.AddDays(-12),
            Tags = new[] { "AI", "technology", "efficiency" }
        });

        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.TechStartup,
            Title = "Remote Work Best Practices",
            Content = "Our team's guide to effective remote collaboration.",
            Author = "HR Director",
            Summary = "Tips for remote work success",
            BlogOwnerNotes = "Consider creating video version for better engagement",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-18),
            PublishedAt = DateTime.UtcNow.AddDays(-18),
            Tags = new[] { "remote-work", "productivity", "team-culture" }
        });

        // MediaCompany blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.MediaCompany,
            Title = "Content Strategy 2025",
            Content = "Our vision for content creation in the coming year.",
            Author = "Content Director",
            Summary = "Strategic planning for 2025",
            BlogOwnerNotes = "Budget allocation: 60% video, 30% written, 10% podcasts",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-3),
            PublishedAt = DateTime.UtcNow.AddDays(-3),
            Tags = new[] { "strategy", "content", "planning" }
        });

        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.MediaCompany,
            Title = "Audience Engagement Metrics",
            Content = "Deep dive into our audience engagement data.",
            Author = "Analytics Team",
            Summary = "Understanding our audience better",
            BlogOwnerNotes = "Confidential: Main competitor analysis included",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            Tags = new[] { "analytics", "engagement", "metrics" }
        });

        return blogs;
    }
}