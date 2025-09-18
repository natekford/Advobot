using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Does not allow managed roles but does allow the everyone role.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotManaged : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Not managed";

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		IRole value)
	{
		if (!value.IsManaged)
		{
			return new(Result.EmptySuccess);
		}
		return new(Result.Failure($"`{value.Format()}` is a managed role."));
	}
}