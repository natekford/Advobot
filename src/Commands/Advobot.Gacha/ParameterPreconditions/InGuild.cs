using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class InGuild : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "In the guild";

		protected override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			object value,
			IServiceProvider services)
		{
			if (value is not IReadOnlyUser user)
			{
				return this.FromOnlySupports(value, typeof(IReadOnlyUser));
			}
			if (await context.Guild.GetUserAsync(user.UserId).CAF() != null)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("The user must be in the guild.");
		}
	}
}