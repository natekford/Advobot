using AdvorangesUtils;

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
public sealed class SoftBan(IGuild guild, ulong userId) : PunishmentBase(guild, userId, true, PunishmentType.Kick)
{

	/// <inheritdoc/>
	protected internal override async Task ExecuteAsync()
	{
		await Guild.AddBanAsync(UserId, Days, Options?.AuditLogReason, Options).CAF();
		await Guild.RemoveBanAsync(UserId, Options).CAF();
	}
}