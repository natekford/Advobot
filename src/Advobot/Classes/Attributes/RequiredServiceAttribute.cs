using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates that the targetted module requires the specified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class RequiredServiceAttribute : PreconditionAttribute
	{
		/// <summary>
		/// The type of service this module requires.
		/// </summary>
		public Type ServiceType { get; }

		/// <summary>
		/// Creates an instance of <see cref="RequiredServiceAttribute"/>.
		/// </summary>
		/// <param name="serviceType"></param>
		public RequiredServiceAttribute(Type serviceType)
		{
			ServiceType = serviceType;
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
			return services.GetService(ServiceType) != null
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError($"The required service `{ServiceType.Name}` is not registered."));
		}
	}
}