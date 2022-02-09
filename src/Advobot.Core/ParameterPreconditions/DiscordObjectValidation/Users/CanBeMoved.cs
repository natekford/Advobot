using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Users;

/// <summary>
/// Checks if the user can be moved from their voice channel.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanBeMoved : AdvobotParameterPrecondition<IGuildUser>
{
	private static readonly ChannelPermission[] _MoveMembers = new[]
	{
		ChannelPermission.MoveMembers
	};

	/// <inheritdoc />
	public override string Summary => "Can be moved from their current channel";

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IGuildUser user,
		IServiceProvider services)
	{
		if (user.VoiceChannel is not IVoiceChannel voiceChannel)
		{
			return PreconditionResult.FromError("The user is not in a voice channel.").AsTask();
		}
		return invoker.ValidateChannel(voiceChannel, _MoveMembers);
	}
}