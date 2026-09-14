using dZENcode.Application.Features.Captcha.DTOs;

namespace dZENcode.Application.Abstractions;

public interface ICaptchaChallengeGenerator
{
    CaptchaChallenge Generate();
}
