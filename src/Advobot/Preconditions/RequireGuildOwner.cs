using Advobot.Modules;

using YACCS.Commands.Models;
using YACCS.Results;

namespace Advobot.Preconditions;

/// <summary>
/// Requires guild owner before this command will execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireGuildOwner : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary => "Invoker is the guild owner";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		if (context.Guild.OwnerId == context.User.Id)
		{
			return new(Result.EmptySuccess);
		}
		return new(Result.Failure("You are not the guild owner."));
	}
}