using System.Diagnostics.CodeAnalysis;
using dZENcode.Application.Features.Comments.DTOs;

namespace dZENcode.API.DTOs.Comments;

// Workaround for the following issue - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0&ref=blog.novanet.no#custom-binding
public class GetCommentsRequest
{
	public Pagination Pagination { get; init; }
	public Sorting Sorting { get; init; }
	
	public static bool TryParse([NotNullWhen(true)] string? s,
		IFormatProvider? provider,
		[MaybeNullWhen(false)] out GetCommentsRequest result)
	{
		string? trimmedValue = s?.TrimStart('(').TrimEnd(')');

		string[]? segments = trimmedValue?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (segments?.Length == 4 
			&& int.TryParse(segments[0], out int page)
			&& int.TryParse(segments[1], out int pageSize)
			&& Enum.TryParse(segments[2], out CommentSortBy sortBy)
			&& Enum.IsDefined(sortBy)
			&& bool.TryParse(segments[3], out bool isDesc))
		{
			Pagination pagination = new (page, pageSize);
			Sorting sorting = new(sortBy, isDesc);

			result = new GetCommentsRequest
			{
				Pagination = pagination,
				Sorting = sorting
			};

			return true;
		}
		
		Pagination defaultPagination = new(Page: 1, PageSize: 25);
		Sorting defaultSorting = new(CommentSortBy.CreatedAt, IsDesc: true);

		result = new()
		{
			Pagination = defaultPagination,
			Sorting = defaultSorting
		};
		
		return true;
	}
	
}

