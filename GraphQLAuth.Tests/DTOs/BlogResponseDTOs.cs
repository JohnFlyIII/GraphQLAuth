namespace GraphQLAuth.Tests.DTOs;

public class BlogsResponse
{
    public List<BlogDto> Blogs { get; set; } = new();
}

public class BlogResponse
{
    public BlogDto? Blog { get; set; }
}

public class BlogDto
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
    public List<string> Tags { get; set; } = new();
}