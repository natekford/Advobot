using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Discord.Invites;

/// <summary>
/// Does not allow invites which can expire.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NeverExpires : AdvobotParameterPrecondition<IInviteMetadata>
{
	/// <inheritdoc />
	public override string Summary => "Never expires";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IInviteMetadata invite,
		IServiceProvider services)
	{
		if (invite.MaxAge is null)
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError("The invite cannot expire.").AsTask();
	}
}