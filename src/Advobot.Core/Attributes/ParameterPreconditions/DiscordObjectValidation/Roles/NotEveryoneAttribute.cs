using System;
using System.Threading.Tasks;

using Advobot.GeneratedParameterPreconditions;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow the everyone role but does allow managed roles.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotEveryoneAttribute : IRoleParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => "Not everyone";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IRole role,
			IServiceProvider services)
		{
			if (context.Guild.EveryoneRole.Id != role.Id)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("The role cannot be the everyone role.").AsTask();
		}
	}
}