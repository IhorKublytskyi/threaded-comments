using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Comments.DTOs;
using dZENcode.Application.Features.Comments.Extensions;
using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Application.Features.Comments.Queries;

public sealed record GetCommentsQuery(
	CommentQueryParameters QueryParameters) : IQuery<PagedResult<CommentDto>>;

public sealed class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, PagedResult<CommentDto>>
{
	private readonly IApplicationDbContext _dbContext;

	public GetCommentsQueryHandler(
		IApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}
	
	public async Task<PagedResult<CommentDto>> HandleAsync(GetCommentsQuery instruction, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		
		IQueryable<Comment> baseQuery = _dbContext.Comments
			.AsNoTracking()
			.Where(x => x.ParentCommentId == null);
		
		int count = await baseQuery
			.CountAsync(cancellationToken);
		
		List<CommentDto> comments = await baseQuery
			.AddSorting(instruction.QueryParameters.Sorting)
			.Skip(instruction.QueryParameters.Pagination.Skip)
			.Take(instruction.QueryParameters.Pagination.PageSize)
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
		
		return new PagedResult<CommentDto>()
		{
			Items = comments,
			Count = count,
		};
	}
}


