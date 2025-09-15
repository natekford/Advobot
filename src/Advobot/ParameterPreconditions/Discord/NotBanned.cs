using Advobot.Modules;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord;

/// <summary>
/// Makes sure the passed in <see cref="ulong"/> is not already banned.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotBanned : AdvobotParameterPrecondition<ulong>
{
	/// <inheritdoc />
	public override string Summary => "Not already banned";

	/// <inheritdoc />
	public override async ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		ulong value)
	{
		var ban = await context.Guild.GetBanAsync(value).ConfigureAwait(false);
		if (ban is null)
		{
			return CachedResults.Success;
		}
		// TODO: singleton
		return Result.Failure($"User is already banned.");
	}
}