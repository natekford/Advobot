using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Makes sure the passed in <see cref="IRole"/> can be modified.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class CanModifyRoleAttribute
		: RoleParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> "Can be modified by both the bot and the invoking user";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckRoleAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IRole role,
			IServiceProvider services)
			=> invoker.ValidateRole(role);
	}
}