namespace dZENcode.Application.Features.Captcha.DTOs;

public sealed record CaptchaResponse(string Token, string ImageBase64, DateTimeOffset ExpiresAtUtc);

