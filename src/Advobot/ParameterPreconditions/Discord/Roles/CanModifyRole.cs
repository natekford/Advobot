using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Makes sure the passed in <see cref="IRole"/> can be modified.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyRole : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		IRole? value
	) => new(context.User.ValidateRole(value));
}