namespace dZENcode.Application.Features.Captcha.DTOs;

public sealed record CaptchaChallengeAnswer(
    string? Token,
    string? Input
);