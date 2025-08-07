using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Discord;

/// <summary>
/// Makes sure the passed in number is not the owner.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotBotOwner : AdvobotParameterPrecondition<ulong>
{
	/// <inheritdoc />
	public override string Summary => "Not the bot owner";

	/// <inheritdoc />
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		ulong value,
		IServiceProvider services)
	{
		var application = await context.Client.GetApplicationInfoAsync().CAF();
		if (application.Owner.Id != value)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("You can't use the bot owner as an argument.");
	}
}