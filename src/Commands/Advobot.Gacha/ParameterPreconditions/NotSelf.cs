
using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.Models;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotSelf : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "Not the invoker";

		protected override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			object value,
			IServiceProvider services)
		{
			if (value is not User user)
			{
				return this.FromOnlySupports(value, typeof(User)).AsTask();
			}
			if (user.GuildId == context.User.Id)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("You cannot use yourself as an argument.").AsTask();
		}
	}
}