using Microsoft.EntityFrameworkCore;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Status> Status { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.BlogOwnerNotes).HasMaxLength(1000);
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.CreatedAt);
            
            // Configure foreign key relationship to Client
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Base64Data);
            entity.Property(e => e.Url).HasMaxLength(2000);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatedAt);
            
            // Configure foreign key relationship to Client
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure many-to-many relationship between Blog and Asset
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Assets)
            .WithMany(a => a.Blogs)
            .UsingEntity<Dictionary<string, object>>(
                "BlogAsset",
                j => j.HasOne<Asset>().WithMany().HasForeignKey("AssetId"),
                j => j.HasOne<Blog>().WithMany().HasForeignKey("BlogId"),
                j =>
                {
                    j.HasKey("BlogId", "AssetId");
                    j.HasIndex("BlogId");
                    j.HasIndex("AssetId");
                });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Health).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();
            entity.Property(e => e.SupportContact).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SystemVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            // Seed default status record
            entity.HasData(new Status
            {
                Id = 1,
                Health = HealthStatus.Healthy,
                LastUpdated = new DateTimeOffset(2025, 6, 5, 20, 0, 0, TimeSpan.Zero),
                SupportContact = "support@demoapp.com",
                SystemVersion = "1.0.0",
                Notes = "System initialized"
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}