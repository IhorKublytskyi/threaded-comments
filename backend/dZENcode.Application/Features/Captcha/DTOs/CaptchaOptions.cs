namespace dZENcode.Application.Features.Captcha.DTOs;

public sealed record CaptchaOptions
{
    public string? SecretKey { get; set;}

    public int TTLMinutes { get; set; }
}
