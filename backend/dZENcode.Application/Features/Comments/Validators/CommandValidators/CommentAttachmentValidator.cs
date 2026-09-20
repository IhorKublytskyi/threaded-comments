using dZENcode.Application.Features.Comments.DTOs;
using FluentValidation;

namespace dZENcode.Application.Features.Comments.Validators.CommandValidators;

public class CommentAttachmentValidator : AbstractValidator<CommentAttachment>
{
	private const int MaxTotalBytes = 10 * 1024 * 1024; // 10 MB
	private const int MaxTxtFileLength = 100 * 1024; // 100 KB

	private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
	{
		".jpg", 
		".jpeg", 
		".png", 
		".gif"
	};

	public CommentAttachmentValidator()
	{
		RuleFor(x => x.Content.Length)
			.LessThanOrEqualTo(MaxTotalBytes)
			.WithMessage("Attachment too large");

		RuleFor(x => x.FileName)
			.Must(IsAllowedExtension)
			.WithMessage("Only JPG, JPEG, PNG, GIF, and TXT files are supported");

		When(x => IsTxtFormat(x.FileName), () =>
		{
			RuleFor(x => x.Content.Length)
				.LessThanOrEqualTo(MaxTxtFileLength)
				.WithMessage("Txt file must not exceed 100 KB");
		});
	}

	private static bool IsAllowedExtension(string fileName)
	{
		string ext = Path.GetExtension(fileName);
		return IsTxtFormat(fileName) || AllowedImageExtensions.Contains(ext);
	}

	private static bool IsTxtFormat(string fileName)
	{
		return Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
	}
}