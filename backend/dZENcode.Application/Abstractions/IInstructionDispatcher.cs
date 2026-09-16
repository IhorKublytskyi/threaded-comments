namespace dZENcode.Application.Abstractions;

public interface IInstructionDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IInstruction<TResponse> instruction, CancellationToken cancellationToken = default);
}
