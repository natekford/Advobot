using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Roles;

/// <summary>
/// Makes sure the passed in <see cref="IRole"/> can be modified.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyRole : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IRole role,
		IServiceProvider services)
		=> invoker.ValidateRole(role);
}