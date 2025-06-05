using GraphQLAuth.Api.Data;
using GraphQLAuth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAuth.Api;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        Console.WriteLine("Resetting and seeding database...");
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Clear all existing data (respecting foreign key constraints)
        await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Assets\"");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Blogs\"");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Clients\"");
        
        Console.WriteLine("Existing data cleared. Adding fresh data...");

        // Create test clients
        var clients = new List<Client>
        {
            new Client
            {
                ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Liz's Pro Bloggers",
                Description = "Professional blogging services for businesses and individuals looking to establish thought leadership and engage their audience with high-quality content."
            },
            new Client
            {
                ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "David's Discount Content Makers",
                Description = "Affordable content creation services for startups and small businesses. Fast turnaround, competitive pricing, and reliable quality."
            },
            new Client
            {
                ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Pauls Premium Blogs",
                Description = "Luxury content creation for enterprise clients. Specializing in executive thought leadership, industry analysis, and premium brand storytelling."
            }
        };

        context.Clients.AddRange(clients);

        // Create test blogs
        var blogs = new List<Blog>
        {
            // Liz's Pro Bloggers blogs
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Professional Blog Writing Tips",
                Content = "Our Q4 results show strong growth across all divisions.",
                Author = "CEO John Smith",
                Summary = "Q4 financial results summary",
                BlogOwnerNotes = "Internal: Revenue exceeded expectations by 15%",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                Tags = new[] { "financial", "quarterly-results", "growth" }
            },
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Content Strategy Guide",
                Content = "We're excited to announce our new product line.",
                Author = "Product Manager",
                Summary = "Introducing our innovative new products",
                BlogOwnerNotes = "Marketing team: Focus on B2B segment for initial launch",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Tags = new[] { "product", "innovation", "announcement" }
            },

            // David's Discount Content blogs
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Budget Content Creation",
                Content = "How we integrated AI into our workflow and improved efficiency by 40%.",
                Author = "CTO Sarah Johnson",
                Summary = "Our journey with AI integration",
                BlogOwnerNotes = "Tech details: Using GPT-4 API with custom fine-tuning",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-12),
                PublishedAt = DateTime.UtcNow.AddDays(-12),
                Tags = new[] { "AI", "technology", "efficiency" }
            },
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Affordable SEO Tips",
                Content = "Our team's guide to effective remote collaboration.",
                Author = "HR Director",
                Summary = "Tips for remote work success",
                BlogOwnerNotes = "Consider creating video version for better engagement",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-18),
                PublishedAt = DateTime.UtcNow.AddDays(-18),
                Tags = new[] { "remote-work", "productivity", "team-culture" }
            },

            // Paul's Premium Blogs blogs
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Executive Content Strategy 2025",
                Content = "Our vision for content creation in the coming year.",
                Author = "Content Director",
                Summary = "Strategic planning for 2025",
                BlogOwnerNotes = "Budget allocation: 60% video, 30% written, 10% podcasts",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                Tags = new[] { "strategy", "content", "planning" }
            },
            new Blog
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Premium Audience Analytics",
                Content = "Deep dive into our audience engagement data.",
                Author = "Analytics Team",
                Summary = "Understanding our audience better",
                BlogOwnerNotes = "Confidential: Main competitor analysis included",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Tags = new[] { "analytics", "engagement", "metrics" }
            }
        };

        context.Blogs.AddRange(blogs);
        await context.SaveChangesAsync(); // Save blogs first to get their IDs

        // Create test assets that match the blog content
        var assets = new List<Asset>();
        var now = DateTime.UtcNow;

        // Get the saved blog IDs for foreign key references
        var lizBlog1 = blogs.First(b => b.ClientId == Guid.Parse("11111111-1111-1111-1111-111111111111") && b.Title.Contains("Professional"));
        var lizBlog2 = blogs.First(b => b.ClientId == Guid.Parse("11111111-1111-1111-1111-111111111111") && b.Title.Contains("Content Strategy"));

        // Liz's Pro Bloggers assets - Professional blogging themed
        var lizAsset1 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Professional Writing Tips Infographic",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-30)
        };
        lizAsset1.SetBase64Data("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        assets.Add(lizAsset1);

        var lizAsset2 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Content Strategy Framework Diagram",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-25),
            UpdatedAt = now.AddDays(-20)
        };
        lizAsset2.SetUrl("https://assets.lizprobloggers.com/content-strategy-framework.png");
        assets.Add(lizAsset2);

        var lizAsset3 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Company Branding Logo",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-40),
            UpdatedAt = now.AddDays(-40)
        };
        lizAsset3.SetBase64Data("iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAYAAABWdVznAAAANUlEQVR4nGNgGAWjYBSMglEwCkbBKBgFo2AUjIJRMApGwSgYBaNgFIyCUTAKRsEoGAX/HwAABQQBAcF7QwAAAABJRU5ErkJggg==");
        assets.Add(lizAsset3);

        // Get David's blog IDs
        var davidBlog1 = blogs.First(b => b.ClientId == Guid.Parse("22222222-2222-2222-2222-222222222222") && b.Title.Contains("Budget"));
        var davidBlog2 = blogs.First(b => b.ClientId == Guid.Parse("22222222-2222-2222-2222-222222222222") && b.Title.Contains("SEO"));

        // David's Discount Content assets - Budget-friendly content themed
        var davidAsset1 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Budget Content Creation Chart",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-18),
            UpdatedAt = now.AddDays(-10)
        };
        davidAsset1.SetUrl("https://content.daviddiscount.com/budget-chart.jpg");
        assets.Add(davidAsset1);

        var davidAsset2 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "SEO Tips Audio Guide",
            Type = AssetType.Audio,
            CreatedAt = now.AddDays(-15),
            UpdatedAt = now.AddDays(-12)
        };
        davidAsset2.SetUrl("https://audio.daviddiscount.com/seo-tips-guide.mp3");
        assets.Add(davidAsset2);

        var davidAsset3 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Affordable Marketing Icon Pack",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-15)
        };
        davidAsset3.SetBase64Data("iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAAFUlEQVR42mNkYPhfz0AEYBxVSF+FAP//8H8e/H8/AAAAAElFTkSuQmCC");
        assets.Add(davidAsset3);

        var davidAsset4 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "AI Integration Success Story Video",
            Type = AssetType.Image, // Thumbnail for video
            CreatedAt = now.AddDays(-16),
            UpdatedAt = now.AddDays(-14)
        };
        davidAsset4.SetUrl("https://content.daviddiscount.com/ai-success-thumbnail.png");
        assets.Add(davidAsset4);

        // Get Paul's blog IDs
        var paulBlog1 = blogs.First(b => b.ClientId == Guid.Parse("33333333-3333-3333-3333-333333333333") && b.Title.Contains("Executive"));
        var paulBlog2 = blogs.First(b => b.ClientId == Guid.Parse("33333333-3333-3333-3333-333333333333") && b.Title.Contains("Analytics"));

        // Paul's Premium Blogs assets - Executive/premium themed
        var paulAsset1 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Executive Strategy 2025 Presentation",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-40),
            UpdatedAt = now.AddDays(-35)
        };
        paulAsset1.SetUrl("https://premium.paulsblog.com/strategy-2025-slide.jpg");
        assets.Add(paulAsset1);

        var paulAsset2 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Premium Analytics Dashboard Screenshot",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-35),
            UpdatedAt = now.AddDays(-30)
        };
        paulAsset2.SetBase64Data("iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAFklEQVR42mNkYGBgYGBgYGBgYGBgYPgPAAUIAAH//wADu4AAAAASUVORK5CYII=");
        assets.Add(paulAsset2);

        var paulAsset3 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Premium Brand Watermark",
            Type = AssetType.Image,
            CreatedAt = now.AddDays(-50),
            UpdatedAt = now.AddDays(-45)
        };
        paulAsset3.SetBase64Data("iVBORw0KGgoAAAANSUhEUgAAAAwAAAAMCAYAAABWdVznAAAANUlEQVR4nGNgGAWjYBSMglEwCkbBKBgFo2AUjIJRMApGwSgYBaNgFIyCUTAKRsEoGAX/HwAABQQBAcF7QwAAAABJRU5ErkJggg==");
        assets.Add(paulAsset3);

        var paulAsset4 = new Asset
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Executive Leadership Podcast Intro",
            Type = AssetType.Audio,
            CreatedAt = now.AddDays(-45),
            UpdatedAt = now.AddDays(-40)
        };
        paulAsset4.SetUrl("https://audio.paulspremium.com/leadership-intro.wav");
        assets.Add(paulAsset4);

        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();
        
        // Now create many-to-many relationships
        lizBlog1.Assets.Add(lizAsset1);
        lizBlog1.Assets.Add(lizAsset3);
        lizBlog2.Assets.Add(lizAsset2);
        
        davidBlog1.Assets.Add(davidAsset1);
        davidBlog1.Assets.Add(davidAsset3);
        davidBlog2.Assets.Add(davidAsset2);
        davidBlog2.Assets.Add(davidAsset4);
        
        paulBlog1.Assets.Add(paulAsset1);
        paulBlog1.Assets.Add(paulAsset3);
        paulBlog1.Assets.Add(paulAsset4);
        paulBlog2.Assets.Add(paulAsset2);
        
        await context.SaveChangesAsync();
        
        Console.WriteLine($"Database reset and seeded successfully! Added {clients.Count} clients, {blogs.Count} blogs, {assets.Count} assets with many-to-many relationships.");
    }
}