using dZENcode.Application.Abstractions;
using FluentValidation;
using FluentValidation.Results;

namespace dZENcode.Application.Features.Dispatcher.Behaviors;

public sealed class ValidationBehavior<TInstruction, TResponse>(IEnumerable<IValidator<TInstruction>> validators) 
	: IPipelineBehavior<TInstruction, TResponse>
	where TInstruction : IInstruction<TResponse>
{
	public async Task<TResponse> HandleAsync(
		TInstruction instruction, 
		InstructionHandlerDelegate<TResponse> next, 
		CancellationToken cancellationToken = default)
	{
		var validatorsList = validators.ToList();

		if(validatorsList.Any() is false)
		{
			return await next();
		}

		var failures = new List<ValidationFailure>();

		foreach(var validator in validatorsList)
		{
			var validationResult = await validator.ValidateAsync(instruction, cancellationToken);

			if(validationResult.IsValid is false)
			{
				failures.AddRange(validationResult.Errors);
			}
		}

		if(failures.Count > 0)
		{
			throw new ValidationException(failures);
		}

		return await next();
	}
}