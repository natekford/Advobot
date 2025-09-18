using Advobot.Modules;

using Discord;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Does not allow the everyone role but does allow managed roles.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotEveryone : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Not everyone";

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		IRole value)
	{
		if (context.Guild.EveryoneRole.Id == value.Id)
		{
			// TODO: singleton
			var error = "The role cannot be the everyone role.";
			return new(Result.Failure(error));
		}
		return new(Result.EmptySuccess);
	}
}