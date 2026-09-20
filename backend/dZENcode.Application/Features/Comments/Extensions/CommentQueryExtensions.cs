using dZENcode.Application.Features.Comments.DTOs;
using dZENcode.Core.Entities;

namespace dZENcode.Application.Features.Comments.Extensions;

public static class CommentQueryExtensions
{
	public static IQueryable<Comment> AddSorting (this IQueryable<Comment> query, Sorting sorting)
	{
		query = sorting.SortBy switch
		{
			CommentSortBy.Username => sorting.IsDesc
				? query.OrderByDescending(x => x.Username).ThenBy(x => x.Id)
				: query.OrderBy(x => x.Username).ThenBy(x => x.Id),
			CommentSortBy.Email=> sorting.IsDesc
				? query.OrderByDescending(x => x.Email).ThenBy(x => x.Id)
				: query.OrderBy(x => x.Email).ThenBy(x => x.Id),
			CommentSortBy.CreatedAt => sorting.IsDesc
				? query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
				: query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
			_ => throw new Exception($"{sorting.SortBy} is not defined in {nameof(Sorting)}")
		};
		

		return query;
	}
}

