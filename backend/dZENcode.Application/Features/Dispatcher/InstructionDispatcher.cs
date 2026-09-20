using dZENcode.Application.Abstractions;

namespace dZENcode.Application.Features.Dispatcher;

internal sealed class InstructionDispatcher(
    IServiceProvider provider, 
    DispatcherRegistry registry) : IInstructionDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(
        IInstruction<TResponse> instruction, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        if(!registry.InstructionWrappers.TryGetValue(instruction.GetType(), out var wrapper))
        {
            throw new InvalidOperationException(
                $"No handler registered for request type '{instruction.GetType().FullName}'.");
        }

        return ((InstructionHandlerBase<TResponse>)wrapper).HandleAsync(instruction, provider, cancellationToken);
    }
}

