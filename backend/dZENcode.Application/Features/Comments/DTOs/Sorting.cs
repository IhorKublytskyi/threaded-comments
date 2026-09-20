using dZENcode.Application.Features.Comments.Extensions;

namespace dZENcode.Application.Features.Comments.DTOs;

public readonly record struct Sorting(CommentSortBy SortBy, bool IsDesc);