using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Mutes a user.
/// </summary>
/// <param name="user"></param>
/// <param name="isGive"></param>
public sealed class VoiceMute(IGuildUser user, bool isGive)
	: PunishmentBase(user.Guild, user.Id, isGive)
{
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.VoiceMute;
	/// <inheritdoc cref="IPunishment.UserId" />
	public IGuildUser User { get; } = user;

	/// <inheritdoc/>
	public override Task ExecuteAsync(RequestOptions? options = null)
		=> User.ModifyAsync(x => x.Mute = IsGive, options);
}