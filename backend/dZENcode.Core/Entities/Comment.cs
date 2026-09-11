namespace dZENcode.Core.Entities;

public class Comment
{
    public int Id { get; set; }

    public int? ParentCommentId { get; set; }

    public required string Email { get; set; }

    public required string Username { get; set; }

    public string? HomePageUrl { get; set; } 

    public required string Body { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? AttachmentPath { get; set; }

    public Comment? Parent { get; set; }

    public ICollection<Comment> Replies { get; set; } = [];
}

