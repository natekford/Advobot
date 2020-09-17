using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class OwnsCharacters : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "Character is owned by the invoker";

		protected override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			object value,
			IServiceProvider services)
		{
			if (!(value is IReadOnlyCharacter character))
			{
				return this.FromOnlySupports(value, typeof(IReadOnlyCharacter));
			}

			var db = services.GetRequiredService<IGachaDatabase>();
			var claim = await db.GetClaimAsync(context.Guild.Id, character).CAF();
			if (claim?.UserId == context.User.Id)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("You do not currently own this character.");
		}
	}
}