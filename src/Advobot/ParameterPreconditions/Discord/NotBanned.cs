using Advobot.Modules;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord;

/// <summary>
/// Makes sure the passed in <see cref="ulong"/> is not already banned.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class NotBanned : AdvobotParameterPrecondition<ulong>
{
	/// <inheritdoc />
	public override string Summary => "Not already banned";

	/// <inheritdoc />
	protected override async ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		ulong value)
	{
		var ban = await context.Guild.GetBanAsync(value).ConfigureAwait(false);
		if (ban is null)
		{
			return Result.EmptySuccess;
		}
		return Result.Failure($"`{value}` is already banned.");
	}
}