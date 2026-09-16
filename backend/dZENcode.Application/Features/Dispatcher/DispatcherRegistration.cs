using System.Collections.Frozen;
using System.Reflection;
using dZENcode.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dZENcode.Application.Features.Dispatcher;

public static class DispatcherRegistration
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddDispatcher(Assembly assembly)
		{
			var instructionWrappers = new Dictionary<Type, InstructionHandlerBase>();

			foreach(var type in assembly.GetTypes())
			{
				if(type.IsAbstract || type.IsInterface) continue;

				foreach(var iface in type.GetInterfaces())
				{
					if(iface.IsGenericType is false) continue;

					var definition = iface.GetGenericTypeDefinition();

					if(definition == typeof(IInstructionHandler<,>))
					{
						services.AddScoped(iface, type);

						var args = iface.GetGenericArguments();

						var instructionType = args[0];
						var responseType = args[1];

						if(instructionWrappers.ContainsKey(instructionType) is false)
						{
							var wrapperType = typeof(InstructionHandlerWrapper<,>)
								.MakeGenericType(instructionType, responseType);

							instructionWrappers[instructionType] = 
								(InstructionHandlerBase)Activator.CreateInstance(wrapperType)!;
						}                    
					}
				}
			}

			var registry = new DispatcherRegistry(
				instructionWrappers.ToFrozenDictionary());

			services.AddSingleton(registry);
			services.AddScoped<InstructionDispatcher>();
			services.AddScoped<IInstructionDispatcher>(sp => sp.GetRequiredService<InstructionDispatcher>());

			return services;
		}

		public IServiceCollection AddPipelineBehavior(Type behaviorType)
		{
			services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);

			return services;
		}
	}
}