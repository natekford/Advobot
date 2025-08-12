using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Softbans a user.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Kick"/>.
/// </remarks>
/// <param name="guild"></param>
/// <param name="userId"></param>
public sealed class SoftBan(IGuild guild, ulong userId)
	: PunishmentBase(guild, userId, true, PunishmentType.Kick)
{
	/// <inheritdoc/>
	public override async Task ExecuteAsync(RequestOptions? options = null)
	{
		await Guild.AddBanAsync(UserId, 1, options?.AuditLogReason, options).ConfigureAwait(false);
		await Guild.RemoveBanAsync(UserId, options).ConfigureAwait(false);
	}
}