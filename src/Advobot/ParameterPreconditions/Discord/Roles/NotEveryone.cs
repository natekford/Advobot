using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Does not allow the everyone role but does allow managed roles.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
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
		if (context.Guild.EveryoneRole.Id != value.Id)
		{
			return new(Result.EmptySuccess);
		}
		return new(Result.Failure($"`{value.Format()}` is the everyone role."));
	}
}