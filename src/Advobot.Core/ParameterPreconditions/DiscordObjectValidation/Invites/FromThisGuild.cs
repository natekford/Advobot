using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Invites;

/// <summary>
/// Only allows invites from this guild.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class FromThisGuild : AdvobotParameterPrecondition<IInviteMetadata>
{
	/// <inheritdoc />
	public override string Summary => "From this guild";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
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