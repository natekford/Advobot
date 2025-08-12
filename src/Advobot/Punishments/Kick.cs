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
	: GuildUserPunishmentBase(user, true)
{
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.Kick;

	/// <inheritdoc />
	public override Task ExecuteAsync(RequestOptions? options = null)
		=> User.KickAsync(options?.AuditLogReason, options);
}