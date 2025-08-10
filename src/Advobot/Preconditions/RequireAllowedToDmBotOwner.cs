using Advobot.Services.BotConfig;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Preconditions;

/// <summary>
/// Checks to make sure the user is allowed to dm the bot owner.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireAllowedToDmBotOwner : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary => "Not blocked by the bot owner";

	/// <inheritdoc />
	public override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		var botConfig = services.GetRequiredService<IRuntimeConfig>();
		if (!botConfig.UsersUnableToDmOwner.Contains(context.User.Id))
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("You are unable to dm the bot owner.").AsTask();
	}
}