using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Context for a punishment being given or removed on a user.
/// </summary>
public abstract class GuildUserPunishmentBase : PunishmentBase
{
	/// <summary>
	/// The user being punished.
	/// </summary>
	public IGuildUser User { get; }

	/// <summary>
	/// Creates an instance of <see cref="GuildUserPunishmentBase"/>.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="isGive"></param>
	/// <param name="type"></param>
	protected GuildUserPunishmentBase(IGuildUser user, bool isGive, PunishmentType type)
		: base(user.Guild, user.Id, isGive, type)
	{
		User = user;
	}
}