using dZENcode.Application.Features.Captcha.DTOs;

namespace dZENcode.API.DTOs.Comments;

public sealed record CreateCommentRequest(
    string? Username,
    string? Email,
    string? HomePageUrl,
    CaptchaChallengeAnswer? CaptchaAnswer,
    int? ParentCommentId,
    string? Body,
    IFormFile? File
);
