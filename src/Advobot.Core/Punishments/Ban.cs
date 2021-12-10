using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Bans a user.
/// </summary>
public sealed class Ban : PunishmentBase
{
	/// <summary>
	/// Creates an instance of <see cref="Deafen"/>.
	/// </summary>
	/// <param name="guild"></param>
	/// <param name="userId"></param>
	/// <param name="isGive"></param>
	public Ban(IGuild guild, ulong userId, bool isGive) : base(guild, userId, isGive, PunishmentType.Ban)
	{
	}

	/// <inheritdoc/>
	protected internal override Task ExecuteAsync()
	{
		if (IsGive)
		{
			return Guild.AddBanAsync(UserId, Days, Options?.AuditLogReason, Options);
		}
		return Guild.RemoveBanAsync(UserId, Options);
	}
}