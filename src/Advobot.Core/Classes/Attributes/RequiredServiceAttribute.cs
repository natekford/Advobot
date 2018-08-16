using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates that the targetted module requires the specified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequiredServices : PreconditionAttribute
	{
		/// <summary>
		/// The types of service this module requires.
		/// </summary>
		public ImmutableArray<Type> ServiceTypes { get; }

		/// <summary>
		/// Creates an instance of <see cref="RequiredServices"/>.
		/// </summary>
		/// <param name="serviceTypes"></param>
		public RequiredServices(params Type[] serviceTypes)
		{
			ServiceTypes = serviceTypes.ToImmutableArray();
		}

		/// <summary>
		/// Makes sure the supplied service is in the service provider.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			foreach (var type in ServiceTypes)
			{
				if (services.GetService(type) == null)
				{
					return Task.FromResult(PreconditionResult.FromError($"The required service `{type.Name}` is not registered."));
				}
			}
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns the names of the required types.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Join(", ", ServiceTypes.Select(x => x.Name));
		}
	}
}