using dZENcode.Application.Features.Comments.DTOs;
using FluentValidation;

namespace dZENcode.Application.Features.Comments.Validators.CommandsValidators;

public class CommentAttachmentValidator : AbstractValidator<CommentAttachment>
{
	private const int MaxTotalBytes = 10 * 1024 * 1024; // 10 MB

	private const int MaxTxtFileLength = 100 * 1024; // 100 KB

	private static readonly HashSet<string> AllowedImageExtensions =
	[
		".jpg", 
		".jpeg", 
		".png", 
		".gif"
	];

	public CommentAttachmentValidator()
	{
		RuleFor(x => x.Content.Length)
			.LessThanOrEqualTo(MaxTotalBytes)
			.WithMessage("Attachment too large");

		When(x => IsTxtFormat(x.FileName), () =>
		{
			RuleFor(x => x.Content.Length)
				.LessThanOrEqualTo(MaxTxtFileLength)
				.WithMessage("Txt file must not exceed 100 KB");
		});

		When(x => IsImageFormat(x.FileName), () =>
		{
			RuleFor(x => x.FileName)
				.Must(f => AllowedImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
				.WithMessage("Only JPG/PNG/GIF supported");
		});
	}

	private static bool IsTxtFormat(string fileName)
	{
		return Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsImageFormat(string fileName)
	{
		string ext = Path.GetExtension(fileName);

		return AllowedImageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
	}
}