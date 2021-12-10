using AdvorangesUtils;

using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Softbans a user.
/// </summary>
public sealed class SoftBan : PunishmentBase
{
	/// <summary>
	/// Creates an instance of <see cref="Kick"/>.
	/// </summary>
	/// <param name="guild"></param>
	/// <param name="userId"></param>
	public SoftBan(IGuild guild, ulong userId) : base(guild, userId, true, PunishmentType.Kick)
	{
	}

	/// <inheritdoc/>
	protected internal override async Task ExecuteAsync()
	{
		await Guild.AddBanAsync(UserId, Days, Options?.AuditLogReason, Options).CAF();
		await Guild.RemoveBanAsync(UserId, Options).CAF();
	}
}