using System.Collections.Frozen;

namespace dZENcode.Application.Features.Dispatcher;

internal sealed class DispatcherRegistry(FrozenDictionary<Type, InstructionHandlerBase> instructionWrappers)
{
	public FrozenDictionary<Type, InstructionHandlerBase> InstructionWrappers { get; } = instructionWrappers;
}