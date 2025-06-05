using System;

namespace GraphQLAuth.Api.Models;

public class Blog
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? BlogOwnerNotes { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    // Navigation property for many-to-many relationship with Assets
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
