using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Preconditions;

/// <summary>
/// Requires bot owner before this command will execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireBotOwner : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary => "Invoker is the bot owner";

	/// <inheritdoc />
	public override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
		if (application.Owner.Id == context.User.Id)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("You are not the bot owner.");
	}
}