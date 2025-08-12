using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Kicks a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Kick"/>.
/// </remarks>
/// <param name="user"></param>
public sealed class Kick(IGuildUser user)
	: GuildUserPunishmentBase(user, true, PunishmentType.Kick)
{
	/// <inheritdoc />
	public override Task ExecuteAsync()
		=> User.KickAsync(Options?.AuditLogReason, Options);
}