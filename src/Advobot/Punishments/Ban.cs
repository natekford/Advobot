using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Bans a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Deafen"/>.
/// </remarks>
/// <param name="guild"></param>
/// <param name="userId"></param>
/// <param name="isGive"></param>
public sealed class Ban(IGuild guild, ulong userId, bool isGive)
	: PunishmentBase(guild, userId, isGive, PunishmentType.Ban)
{
	/// <inheritdoc/>
	public override Task ExecuteAsync()
	{
		if (IsGive)
		{
			return Guild.AddBanAsync(UserId, 1, Options?.AuditLogReason, Options);
		}
		return Guild.RemoveBanAsync(UserId, Options);
	}
}