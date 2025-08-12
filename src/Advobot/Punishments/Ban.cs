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
	: PunishmentBase(guild, userId, isGive)
{
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.Ban;

	/// <inheritdoc/>
	public override Task ExecuteAsync(RequestOptions? options = null)
	{
		if (IsGive)
		{
			return Guild.AddBanAsync(UserId, 1, options?.AuditLogReason, options);
		}
		return Guild.RemoveBanAsync(UserId, options);
	}
}