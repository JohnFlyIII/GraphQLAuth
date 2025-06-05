using System;
using System.ComponentModel.DataAnnotations;

namespace GraphQLAuth.Api.Models;

public enum AssetType
{
    Image,
    Audio
}

public class Asset
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public string? Base64Data { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void SetBase64Data(string base64Data)
    {
        Base64Data = base64Data;
        Url = null;
    }

    public void SetUrl(string url)
    {
        Url = url;
        Base64Data = null;
    }

    public bool IsValid()
    {
        return (Base64Data != null && Url == null) || (Base64Data == null && Url != null);
    }
    
    // Navigation property for many-to-many relationship with Blogs
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}