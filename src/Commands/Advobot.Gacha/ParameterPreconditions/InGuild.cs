using Advobot.Gacha.Models;
using Advobot.ParameterPreconditions;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class InGuild : AdvobotParameterPrecondition<User>
{
	public override string Summary => "In the guild";

	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		User value,
		IServiceProvider services)
	{
		if (await context.Guild.GetUserAsync(value.UserId).CAF() != null)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("The user must be in the guild.");
	}
}