using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions;

/// <summary>
/// Will return success if the bot is the owner of the guild in the context.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireBotIsOwnerAttribute
	: PreconditionAttribute, IPrecondition
{
	/// <inheritdoc />
	public string Summary
		=> "Bot owns the current guild";

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