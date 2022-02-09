using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Users;

/// <summary>
/// Validates the passed in <see cref="IGuildUser"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyUser : AdvobotParameterPrecondition<IGuildUser>
{
	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IGuildUser user,
		IServiceProvider services)
		=> invoker.ValidateUser(user);
}