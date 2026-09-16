namespace dZENcode.Application.Abstractions;

public interface ICommandHandler<TCommand, TResponse> : IInstructionHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>   
{
    
}
