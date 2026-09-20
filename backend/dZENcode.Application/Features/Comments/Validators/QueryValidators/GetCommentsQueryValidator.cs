using dZENcode.Application.Features.Comments.Queries;
using FluentValidation;

namespace dZENcode.Application.Features.Comments.Validators.QueryValidators;

public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
	public GetCommentsQueryValidator()
	{
		RuleFor(x => x.QueryParameters.Pagination)
			.NotNull().WithMessage("Pagination parameters are required");

		RuleFor(x => x.QueryParameters.Sorting)
			.NotNull().WithMessage("Sorting parameters are required");

		RuleFor(x => x.QueryParameters.Sorting.SortBy)
			.IsInEnum();
		
		RuleFor(x => x.QueryParameters.Pagination.PageSize)
			.InclusiveBetween(1, 25).WithMessage("PageSize must be between 0 and 25");
		
		RuleFor(x => x.QueryParameters.Pagination.Page)
			.GreaterThan(0).WithMessage("Page must be greater than 0");
		
	}
}