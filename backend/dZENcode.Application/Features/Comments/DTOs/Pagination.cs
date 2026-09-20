namespace dZENcode.Application.Features.Comments.DTOs;

public readonly record struct Pagination(int Page, int PageSize)
{
	public int Skip => (Page - 1) * PageSize;
}