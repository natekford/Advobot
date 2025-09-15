using Advobot.Modules;

using YACCS.Commands.Models;
using YACCS.Results;

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
	public override async ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
		if (application.Owner.Id == context.User.Id)
		{
			return CachedResults.Success;
		}
		// TODO: singleton?
		return Result.Failure("You are not the bot owner.");
	}
}