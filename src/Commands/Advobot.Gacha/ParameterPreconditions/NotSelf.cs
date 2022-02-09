using Advobot.Gacha.Models;
using Advobot.ParameterPreconditions;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotSelf : AdvobotParameterPrecondition<User>
{
	public override string Summary => "Not the invoker";

	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		User value,
		IServiceProvider services)
	{
		if (value.GuildId == context.User.Id)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("You cannot use yourself as an argument.").AsTask();
	}
}