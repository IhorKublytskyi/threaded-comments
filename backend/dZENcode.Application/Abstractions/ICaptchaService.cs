using dZENcode.Application.Features.Captcha.DTOs;

namespace dZENcode.Application.Abstractions;

public interface ICaptchaService
{
    ValueTask<CaptchaResponse> GenerateAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> ValidateAsync(string? token, string? input, CancellationToken cancellationToken = default);
}