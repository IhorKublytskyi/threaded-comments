namespace dZENcode.Application.Abstractions;

public interface IQueryHandler<TQuery, TResponse> : IInstructionHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
