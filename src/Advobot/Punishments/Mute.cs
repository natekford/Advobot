using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Mutes a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Mute"/>.
/// </remarks>
/// <param name="user"></param>
/// <param name="isGive"></param>
public sealed class Mute(IGuildUser user, bool isGive) : GuildUserPunishmentBase(user, isGive, PunishmentType.VoiceMute)
{
	/// <inheritdoc/>
	public override Task ExecuteAsync()
		=> User.ModifyAsync(x => x.Mute = IsGive, Options);
}