using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Users;

/// <summary>
/// Checks if the user can be moved from their voice channel.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanBeMoved : AdvobotParameterPrecondition<IGuildUser>
{
	private static readonly ChannelPermission[] _MoveMembers =
	[
		ChannelPermission.MoveMembers
	];

	/// <inheritdoc />
	public override string Summary => "Can be moved from their current channel";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		IGuildUser? value)
	{
		if (value?.VoiceChannel is not IVoiceChannel voiceChannel)
		{
			return new(Result.Failure("The user is not in a voice channel."));
		}
		return new(context.User.ValidateChannel(voiceChannel, _MoveMembers));
	}
}