using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Preconditions;

/// <summary>
/// Will return success if the bot is the owner of the guild in the context.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireBotIsOwner : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary => "Bot owns the current guild";

	/// <inheritdoc />
	public override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		if (context.Client.CurrentUser.Id == context.Guild.OwnerId)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("The bot is not the owner of the guild.").AsTask();
	}
}