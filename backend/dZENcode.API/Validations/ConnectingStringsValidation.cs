using System.Text;
using Microsoft.Extensions.Options;

namespace dZENcode.API.Validations;

public sealed class ConnectingStringsValidation : IValidateOptions<ConnectionStrings>
{
	public ValidateOptionsResult Validate(string? name, ConnectionStrings options)
	{
		if (options is null)
		{
			return ValidateOptionsResult.Fail($"{nameof(ConnectionStrings)} not found\n");
		}

		StringBuilder validationResult = new();

		if (string.IsNullOrWhiteSpace(options.DzenConnectionString))
		{
			validationResult.Append($"{nameof(ConnectionStrings.DzenConnectionString)} is missing\n");
		}

		if (string.IsNullOrWhiteSpace(options.RedisConnectionString))
		{
			validationResult.Append($"{nameof(ConnectionStrings.RedisConnectionString)} is missing\n");
		}

		if (validationResult.Length > 0)
		{
			return ValidateOptionsResult.Fail(validationResult.ToString());
		}
		
		return ValidateOptionsResult.Success;
	}
}