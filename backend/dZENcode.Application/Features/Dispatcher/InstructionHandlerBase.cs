using dZENcode.Application.Abstractions;

namespace dZENcode.Application.Features.Dispatcher;

internal abstract class InstructionHandlerBase;

internal abstract class InstructionHandlerBase<TResponse> : InstructionHandlerBase
{
	public abstract Task<TResponse> HandleAsync(
		IInstruction<TResponse> instruction, 
		IServiceProvider serviceProvider,
		CancellationToken cancellationToken = default);
};