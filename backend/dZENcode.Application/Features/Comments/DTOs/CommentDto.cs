namespace dZENcode.Application.Features.Comments.DTOs;

public record CommentDto
{
	public int Id { get; init; }

	public string? Email { get; init; }

	public string? Username { get; init; }

	public string? Body { get; init; }

	public DateTimeOffset CreatedAt { get; init; }

	public string? AttachmentPath { get; init; }

	public int RepliesCount { get; init; }
}