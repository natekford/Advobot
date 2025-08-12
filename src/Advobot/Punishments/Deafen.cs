using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Deafens a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Deafen"/>.
/// </remarks>
/// <param name="user"></param>
/// <param name="isGive"></param>
public sealed class Deafen(IGuildUser user, bool isGive)
	: GuildUserPunishmentBase(user, isGive, PunishmentType.Deafen)
{
	/// <inheritdoc/>
	public override Task ExecuteAsync(RequestOptions? options = null)
		=> User.ModifyAsync(x => x.Deaf = IsGive, options);
}