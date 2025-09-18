using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Users;

/// <summary>
/// Validates the passed in <see cref="IGuildUser"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyUser : AdvobotParameterPrecondition<IGuildUser>
{
	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		IGuildUser value
	) => new(context.User.ValidateUser(value));
}