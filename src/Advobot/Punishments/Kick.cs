using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Kicks a user.
/// </summary>
/// <param name="user"></param>
public sealed class Kick(IGuildUser user)
	: PunishmentBase(user.Guild, user.Id, true)
{
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.Kick;
	/// <inheritdoc cref="IPunishment.UserId" />
	public IGuildUser User { get; } = user;

	/// <inheritdoc />
	public override Task ExecuteAsync(RequestOptions? options = null)
	{
		if (IsGive)
		{
			return User.KickAsync(options?.AuditLogReason, options);
		}
		return Task.CompletedTask;
	}
}