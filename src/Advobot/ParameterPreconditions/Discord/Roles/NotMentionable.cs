using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Discord.Roles;

/// <summary>
/// Does not allow roles which are already mentionable to be used.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotMentionable : AdvobotParameterPrecondition<IRole>
{
	/// <inheritdoc />
	public override string Summary => "Not mentionable";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IRole role,
		IServiceProvider services)
	{
		if (!role.IsMentionable)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("The role cannot be mentionable.").AsTask();
	}
}