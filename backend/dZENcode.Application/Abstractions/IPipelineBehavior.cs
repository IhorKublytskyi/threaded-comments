using dZENcode.Application.Features.Dispatcher;

namespace dZENcode.Application.Abstractions;

public interface IPipelineBehavior<in TInstruction, TResponse> 
	where TInstruction : IInstruction<TResponse>
{
	Task<TResponse> HandleAsync(
		TInstruction instruction,
		InstructionHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken = default);
}