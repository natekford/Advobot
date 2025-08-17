using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Punishes based off of <see cref="IPunishment.Type"/>.
/// </summary>
/// <param name="guild"></param>
/// <param name="userId"></param>
/// <param name="isGive"></param>
/// <param name="type"></param>
public sealed class DynamicPunishment(IGuild guild, ulong userId, bool isGive, PunishmentType type)
	: PunishmentBase(guild, userId, isGive)
{
	/// <inheritdoc />
	public override PunishmentType Type => type;

	/// <inheritdoc/>
	public override async Task ExecuteAsync(RequestOptions? options = null)
	{
		var context = await GetPunishmentAsync().ConfigureAwait(false);
		if (context is null)
		{
			return;
		}

		context.Duration = Duration;
		await context.ExecuteAsync(options).ConfigureAwait(false);
	}

	private async Task<PunishmentBase?> GetPunishmentAsync()
	{
		var user = await Guild.GetUserAsync(UserId).ConfigureAwait(false);
		return Type switch
		{
			PunishmentType.Ban => new Ban(Guild, UserId, IsGive),
			PunishmentType.Deafen => new Deafen(user, IsGive),
			PunishmentType.Kick => new Kick(user),
			PunishmentType.RoleMute => new RoleMute(user, Guild.GetRole(RoleId), IsGive),
			PunishmentType.Softban => new SoftBan(Guild, UserId),
			PunishmentType.VoiceMute => new VoiceMute(user, IsGive),
			_ => null,
		};
	}
}