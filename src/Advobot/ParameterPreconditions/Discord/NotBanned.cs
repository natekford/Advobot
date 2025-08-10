using Advobot.Utilities;

using Discord;
using Discord.Commands;

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
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		ulong value,
		IServiceProvider services)
	{
		var ban = await context.Guild.GetBanAsync(value).ConfigureAwait(false);
		if (ban is null)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError($"`{value}` already exists as a ban.");
	}
}