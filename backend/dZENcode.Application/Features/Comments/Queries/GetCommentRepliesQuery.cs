using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Comments.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Application.Features.Comments.Queries;

public sealed record GetCommentRepliesQuery(
	int CommentId) : IQuery<List<CommentDto>>;

public sealed class GetCommentRepliesQueryHandler : IQueryHandler<GetCommentRepliesQuery, List<CommentDto>>
{
	private readonly IApplicationDbContext _dbContext;

	public GetCommentRepliesQueryHandler(IApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<List<CommentDto>> HandleAsync(
		GetCommentRepliesQuery instruction, 
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		List<CommentDto> replies = await _dbContext.Comments
			.AsNoTracking()
			.Where(x => x.ParentCommentId == instruction.CommentId)
			.OrderBy(x => x.CreatedAt)
			.Select(x => new CommentDto()
			{
				Id = x.Id,
				Username = x.Username,
				Email = x.Email,
				Body = x.Body,
				AttachmentPath = x.AttachmentPath,
				CreatedAt = x.CreatedAt,
				RepliesCount = x.Replies.Count
			})
			.ToListAsync(cancellationToken);
		
		return replies;
	}
}