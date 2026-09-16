using dZENcode.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dZENcode.Application.Features.Dispatcher;

public delegate Task<TResponse> InstructionHandlerDelegate<TResponse>();

internal sealed class InstructionHandlerWrapper<TInstruction, TResponse> : InstructionHandlerBase<TResponse>
	where TInstruction : IInstruction<TResponse>
{
	public override Task<TResponse> HandleAsync(
		IInstruction<TResponse> instruction, 
		IServiceProvider serviceProvider, 
		CancellationToken cancellationToken = default)
	{
		var instructionType = (TInstruction)instruction;
		var handler = serviceProvider.GetRequiredService<IInstructionHandler<TInstruction, TResponse>>();
		var behaviors = serviceProvider.GetServices<IPipelineBehavior<TInstruction, TResponse>>();

		InstructionHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(instructionType, cancellationToken);

		foreach(var behavior in behaviors.Reverse())
		{
			var next = pipeline;
			var current = behavior;

			pipeline = () => current.HandleAsync(instructionType, next, cancellationToken);
		}

		return pipeline();
	}
}