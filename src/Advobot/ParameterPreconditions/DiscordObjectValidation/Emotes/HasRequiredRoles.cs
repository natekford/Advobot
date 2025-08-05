using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Emotes;

/// <summary>
/// Requires the guild emote have roles required to use it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class HasRequiredRoles : AdvobotParameterPrecondition<GuildEmote>
{
	/// <inheritdoc />
	public override string Summary => "Has required roles";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		GuildEmote emote,
		IServiceProvider services)
	{
		if (emote.RoleIds.Count > 0)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("The emote must have required roles.").AsTask();
	}
}