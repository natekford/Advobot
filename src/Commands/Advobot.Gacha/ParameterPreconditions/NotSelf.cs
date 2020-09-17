using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotSelf : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "Not the invoker";

		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is IReadOnlyUser user))
			{
				return this.FromOnlySupports(value, typeof(IReadOnlyUser)).AsTask();
			}
			else if (user.GuildId == context.User.Id)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("You cannot use yourself as an argument.").AsTask();
		}
	}
}