using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.ParameterPreconditions;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class OwnsCharacters : AdvobotParameterPrecondition<Character>
{
	public override string Summary => "Character is owned by the invoker";

	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		Character value,
		IServiceProvider services)
	{
		var db = services.GetRequiredService<IGachaDatabase>();
		var claim = await db.GetClaimAsync(context.Guild.Id, value).CAF();
		if (claim?.UserId == context.User.Id)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError("You do not currently own this character.");
	}
}