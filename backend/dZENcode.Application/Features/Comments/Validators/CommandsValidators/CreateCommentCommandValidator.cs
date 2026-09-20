using System.Xml;
using dZENcode.Application.Features.Comments.Commands;
using FluentValidation;

namespace dZENcode.Application.Features.Comments.Validators.CommandsValidators;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email can't be empty")
            .MaximumLength(256).WithMessage("Max email length is 256")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username can't be empty")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Username should only contain latin letters and digits")
            .MaximumLength(64).WithMessage("Max username length is 64");

        RuleFor(x => x.HomePageUrl)
            .MaximumLength(256).WithMessage("Max homepage url length is 256")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri? u) && u.Scheme is "http" or "https")
            .When(x => string.IsNullOrWhiteSpace(x.HomePageUrl) is not true).WithMessage("Invalid link format");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment can't be empty")
            .MaximumLength(1024).WithMessage("Max comment body length is 1024");
        
        When(x => string.IsNullOrWhiteSpace(x.Body) is false, () =>
        {
            RuleFor(x => x.Body)
                .Custom((body, ctx) =>
                {
                    try
                    {
                        new XmlDocument().LoadXml($"<root>{body}</root>");
                    }
                    catch (XmlException ex)
                    {
                        ctx.AddFailure(nameof(CreateCommentCommand.Body), 
                            $"Invalid XHTML: {ex.Message}");
                    }
                });
        });
        
        RuleFor(x => x.CaptchaChallengeAnswer).NotNull().WithMessage("Captcha answer can't be null");
        
        RuleFor(x => x.CaptchaChallengeAnswer.Token)
            .NotEmpty().WithMessage("Captcha token can't be empty")
            .Matches("^[a-zA-Z0-9-]+$").WithMessage("Captcha token should only contain latin letters and digits");
        
        RuleFor(x => x.CaptchaChallengeAnswer.Input)
            .NotEmpty().WithMessage("Captcha answer can't be empty")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Captcha answer should only contain latin letters and digits");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0)
            .When(x => x.ParentCommentId.HasValue).WithMessage("Parent id can't be less than or equal to '0'");

        RuleFor(x => x.Attachment)
            .SetValidator(new CommentAttachmentValidator())
            .When(x => x.Attachment is not null);
    }
}
