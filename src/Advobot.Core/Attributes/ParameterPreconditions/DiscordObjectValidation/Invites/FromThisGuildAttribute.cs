using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which are not from this guild.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class FromThisGuildAttribute
		: InviteParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> "From this guild";

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckInviteAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IInviteMetadata invite,
			IServiceProvider services)
		{
			if (context.Guild.Id == invite.GuildId)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("The invite must belong to this guild.").AsTask();
		}
	}
}