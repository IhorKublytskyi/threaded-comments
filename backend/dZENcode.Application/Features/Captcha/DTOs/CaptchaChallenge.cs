namespace dZENcode.Application.Features.Captcha.DTOs;

public sealed record CaptchaChallenge(string Body, byte[] Image);
