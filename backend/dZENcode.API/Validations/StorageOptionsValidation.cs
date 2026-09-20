using System.Text;
using dZENcode.Application.Features.Comments.DTOs;
using Microsoft.Extensions.Options;

namespace dZENcode.API.Validations;

public sealed class StorageOptionsValidation : IValidateOptions<StorageOptions>
{
	public ValidateOptionsResult Validate(string? name, StorageOptions options)
	{
		if (options is null)
		{
			return ValidateOptionsResult.Fail($"{nameof(StorageOptions)} not found\n");
		}

		StringBuilder validationResult = new();

		if (string.IsNullOrWhiteSpace(options.RootPath))
		{
			validationResult.Append($"{nameof(StorageOptions.RootPath)} can't be empty\n");
		}

		if (validationResult.Length > 0)
		{
			return ValidateOptionsResult.Fail(validationResult.ToString());
		}

		return ValidateOptionsResult.Success;
	}
}