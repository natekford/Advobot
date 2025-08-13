using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Softbans a user.
/// </summary>
/// <param name="guild"></param>
/// <param name="userId"></param>
public sealed class SoftBan(IGuild guild, ulong userId)
	: PunishmentBase(guild, userId, true)
{
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.Softban;

	/// <inheritdoc/>
	public override async Task ExecuteAsync(RequestOptions? options = null)
	{
		if (!IsGive)
		{
			return;
		}

		await Guild.AddBanAsync(UserId, 1, options?.AuditLogReason, options).ConfigureAwait(false);
		await Guild.RemoveBanAsync(UserId, options).ConfigureAwait(false);
	}
}