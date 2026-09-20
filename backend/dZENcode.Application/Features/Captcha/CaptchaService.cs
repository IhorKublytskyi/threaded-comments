using System.Security.Cryptography;
using System.Text;
using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha.DTOs;
using Microsoft.Extensions.Options;

namespace dZENcode.Application.Features.Captcha;

public class CaptchaService : ICaptchaService
{
    private readonly ICaptchaChallengeGenerator _captchaChallengeGenerator;

    private readonly ICaptchaStorage _captchaStorage;

    private readonly CaptchaOptions _captchaOptions;

    public CaptchaService(
        ICaptchaChallengeGenerator captchaChallengeGenerator,
        ICaptchaStorage captchaStorage,
        IOptions<CaptchaOptions> captchaOptions)
    {
        _captchaChallengeGenerator = captchaChallengeGenerator;
        _captchaStorage = captchaStorage;
        _captchaOptions = captchaOptions.Value;
    }

    public async ValueTask<CaptchaResponse> GenerateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CaptchaChallenge challenge = _captchaChallengeGenerator.Generate();

        string token = Guid.NewGuid().ToString();
        
        byte[] hash = ComputeHash(challenge.Body);

        await _captchaStorage.CreateEntryAsync
        (
            new CaptchaEntry
            (
                token, 
                hash, 
                _captchaOptions.TTLMinutes
            ),
            cancellationToken);

        return new CaptchaResponse(
            token, 
            Convert.ToBase64String(challenge.Image), 
            DateTimeOffset.UtcNow.AddMinutes(_captchaOptions.TTLMinutes));
    }

    public async ValueTask<bool> ValidateAsync(string? token, string? input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        byte[]? hashedAnswer = await _captchaStorage.GetAndRemoveAsync(token, cancellationToken);

        if(hashedAnswer is null)
        {
            return false;
        }

        byte[] hashedInput = ComputeHash(input);

        return CryptographicOperations.FixedTimeEquals(hashedAnswer, hashedInput);
    }

    private byte[] ComputeHash(string answer)
    {
        byte[] data = Encoding.UTF8.GetBytes(answer.Trim());
        byte[] key = Encoding.UTF8.GetBytes(_captchaOptions.SecretKey);

        return HMACSHA256.HashData(key, data);
    }
}