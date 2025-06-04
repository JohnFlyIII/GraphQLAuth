using Microsoft.EntityFrameworkCore;
using GraphQLAuth.Api.Models;

namespace GraphQLAuth.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; } = null!;

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
        });

        base.OnModelCreating(modelBuilder);
    }
}