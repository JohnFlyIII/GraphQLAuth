using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Tests.Infrastructure;

public static class TestDataFactory
{
    public static class Clients
    {
        public static readonly Guid LizProBloggers = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid DavidsDiscountContent = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid PaulsPremiumBlogs = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }

    public static List<Client> CreateTestClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = Clients.LizProBloggers,
                Name = "Liz's Pro Bloggers",
                Description = "Professional blogging services for businesses and individuals looking to establish thought leadership and engage their audience with high-quality content."
            },
            new Client
            {
                ClientId = Clients.DavidsDiscountContent,
                Name = "David's Discount Content Makers",
                Description = "Affordable content creation services for startups and small businesses. Fast turnaround, competitive pricing, and reliable quality."
            },
            new Client
            {
                ClientId = Clients.PaulsPremiumBlogs,
                Name = "Pauls Premium Blogs",
                Description = "Luxury content creation for enterprise clients. Specializing in executive thought leadership, industry analysis, and premium brand storytelling."
            }
        };
    }

    public static List<Blog> CreateTestBlogs()
    {
        var blogs = new List<Blog>();

        // Liz's Pro Bloggers blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.LizProBloggers,
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
            ClientId = Clients.LizProBloggers,
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

        // David's Discount Content blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.DavidsDiscountContent,
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
            ClientId = Clients.DavidsDiscountContent,
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

        // Paul's Premium Blogs blogs
        blogs.Add(new Blog
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.PaulsPremiumBlogs,
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
            ClientId = Clients.PaulsPremiumBlogs,
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

    public static List<Asset> CreateTestAssets()
    {
        var assets = new List<Asset>();
        var now = DateTime.UtcNow;

        // Liz's Pro Bloggers assets
        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.LizProBloggers,
            Name = "Company Logo",
            Type = AssetType.Image,
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-30)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.LizProBloggers,
            Name = "Hero Banner",
            Type = AssetType.Image,
            Url = "https://example.com/images/liz-hero-banner.jpg",
            CreatedAt = now.AddDays(-25),
            UpdatedAt = now.AddDays(-20)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.LizProBloggers,
            Name = "Podcast Intro",
            Type = AssetType.Audio,
            Base64Data = "UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmEe",
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-15)
        });

        // David's Discount Content assets
        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.DavidsDiscountContent,
            Name = "Budget Template",
            Type = AssetType.Image,
            Url = "https://example.com/templates/budget-template.png",
            CreatedAt = now.AddDays(-18),
            UpdatedAt = now.AddDays(-10)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.DavidsDiscountContent,
            Name = "Social Media Icon",
            Type = AssetType.Image,
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAAFUlEQVR42mNkYPhfz0AEYBxVSF+FAP//8H8e/H8/AAAAAElFTkSuQmCC",
            CreatedAt = now.AddDays(-15),
            UpdatedAt = now.AddDays(-12)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.DavidsDiscountContent,
            Name = "Background Music",
            Type = AssetType.Audio,
            Url = "https://example.com/audio/background-music.mp3",
            CreatedAt = now.AddDays(-12),
            UpdatedAt = now.AddDays(-8)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.DavidsDiscountContent,
            Name = "Call-to-Action Button",
            Type = AssetType.Image,
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAKklEQVR4nGNgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYPgPAAMAAP//8AAuAAAAAElFTkSuQmCC",
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now.AddDays(-5)
        });

        // Paul's Premium Blogs assets
        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.PaulsPremiumBlogs,
            Name = "Premium Brand Logo",
            Type = AssetType.Image,
            Url = "https://cdn.premium-assets.com/logos/paul-premium-logo.svg",
            CreatedAt = now.AddDays(-40),
            UpdatedAt = now.AddDays(-35)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.PaulsPremiumBlogs,
            Name = "Executive Portrait",
            Type = AssetType.Image,
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAFklEQVR42mNkYGBgYGBgYGBgYGBgYPgPAAUIAAH//wADu4AAAAASUVORK5CYII=",
            CreatedAt = now.AddDays(-35),
            UpdatedAt = now.AddDays(-30)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.PaulsPremiumBlogs,
            Name = "Brand Jingle",
            Type = AssetType.Audio,
            Url = "https://premium-audio.com/jingles/paul-premium-jingle.wav",
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-25)
        });

        assets.Add(new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Clients.PaulsPremiumBlogs,
            Name = "Watermark",
            Type = AssetType.Image,
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAYAAABWdVznAAAANUlEQVR4nGNgGAWjYBSMglEwCkbBKBgFo2AUjIJRMApGwSgYBaNgFIyCUTAKRsEoGAX/HwAABQQBAcF7QwAAAABJRU5ErkJggg==",
            CreatedAt = now.AddDays(-25),
            UpdatedAt = now.AddDays(-20)
        });

        return assets;
    }
}