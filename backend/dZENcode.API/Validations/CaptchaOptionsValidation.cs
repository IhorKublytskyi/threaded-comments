using System.Text;
using dZENcode.Application.Features.Captcha.DTOs;
using Microsoft.Extensions.Options;

namespace dZENcode.API.Validations;

public sealed class CaptchaOptionsValidation : IValidateOptions<CaptchaOptions>
{
	public ValidateOptionsResult Validate(string? name, CaptchaOptions options)
	{
		if (options is null)
		{
			return ValidateOptionsResult.Fail($"{nameof(CaptchaOptions)} not found\n");
		}

		StringBuilder validationResult = new();

		if (string.IsNullOrWhiteSpace(options.SecretKey) || options.SecretKey.Length < 16)
		{
			validationResult.Append($"{nameof(CaptchaOptions.SecretKey)} must be at least 16 characters\n");
		}

		if (options.TTLMinutes <= 0)
		{
			validationResult.Append($"{nameof(CaptchaOptions.TTLMinutes)} must be greater than 0\n");
		}
		
		if (validationResult.Length > 0)
		{
			return ValidateOptionsResult.Fail(validationResult.ToString());
		}

		return ValidateOptionsResult.Success;
	}
}