using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Validates the passed in <see cref="IRole"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class RoleParameterPreconditionAttribute
		: SnowflakeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(IRole),
		}.ToImmutableArray();

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return this.FromInvalidInvoker().AsTask();
			}
			if (!(value is IRole role))
			{
				return this.FromOnlySupports(value).AsTask();
			}
			return SingularCheckRoleAsync(context, parameter, invoker, role, services);
		}

		/// <summary>
		/// Checks whether the condition for the <see cref="IRole"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="role"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckRoleAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IRole role,
			IServiceProvider services);
	}
}