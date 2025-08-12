using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Context for a punishment being given or removed on a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="GuildUserPunishmentBase"/>.
/// </remarks>
/// <param name="user"></param>
/// <param name="isGive"></param>
public abstract class GuildUserPunishmentBase(IGuildUser user, bool isGive)
	: PunishmentBase(user.Guild, user.Id, isGive)
{
	/// <summary>
	/// The user being punished.
	/// </summary>
	public IGuildUser User { get; } = user;
}