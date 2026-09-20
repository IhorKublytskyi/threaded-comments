using dZENcode.Application.Features.Comments.Queries;
using FluentValidation;

namespace dZENcode.Application.Features.Comments.Validators.QueryValidators;

public class GetCommentRepliesQueryValidator : AbstractValidator<GetCommentRepliesQuery>
{
	public GetCommentRepliesQueryValidator()
	{
		RuleFor(x => x.CommentId)
			.GreaterThan(0);
	}
}