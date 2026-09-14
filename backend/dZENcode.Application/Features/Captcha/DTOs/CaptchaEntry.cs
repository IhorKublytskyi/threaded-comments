namespace dZENcode.Application.Features.Captcha.DTOs;

public sealed record CaptchaEntry(string Token, byte[] HashedAnswer, int TTLMinutes);
