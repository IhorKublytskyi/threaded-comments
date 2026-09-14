using dZENcode.Application.Features.Captcha.DTOs;

namespace dZENcode.Application.Abstractions;

public interface ICaptchaStorage
{
    ValueTask CreateEntryAsync(CaptchaEntry entry, CancellationToken cancellationToken = default);

    ValueTask<byte[]?> GetAndRemoveAsync(string token, CancellationToken cancellationToken = default);
}
