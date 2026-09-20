using dZENcode.Application.Abstractions;
using Ganss.Xss;

namespace dZENcode.Application.Features.Comments;

public sealed class HtmlSanitizerService : IHtmlSanitizerService
{
	private readonly HtmlSanitizer _sanitizer = new(new HtmlSanitizerOptions
	{
		AllowedTags = new HashSet<string> { "a", "code", "i", "strong" },
		AllowedAttributes = new HashSet<string> { "href", "title" },
		AllowedSchemes = new HashSet<string> { "http", "https" }
	});

	public string Sanitize(string html) => _sanitizer.Sanitize(html);
}