namespace dZENcode.Application.Abstractions;

public interface IInstructionHandler<TInstruction, TResponse>
{
    Task<TResponse> HandleAsync(TInstruction instruction, CancellationToken cancellationToken = default);
}
