namespace dZENcode.Application.Features.Comments.DTOs;

public sealed record CommentQueryParameters(
	Pagination Pagination,
	Sorting Sorting);