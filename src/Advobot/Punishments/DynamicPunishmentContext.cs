using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Punishes based off of <see cref="IPunishmentContext.Type"/>.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="DynamicPunishmentContext"/>.
/// </remarks>
/// <param name="guild"></param>
/// <param name="userId"></param>
/// <param name="isGive"></param>
/// <param name="type"></param>
public sealed class DynamicPunishmentContext(IGuild guild, ulong userId, bool isGive, PunishmentType type) : PunishmentBase(guild, userId, isGive, type)
{
	/// <summary>
	/// The id of the role.
	/// </summary>
	public ulong RoleId
	{
		get => Role?.Id ?? 0;
		set => Role = Guild.GetRole(value);
	}

	/// <inheritdoc/>
	protected internal override async Task ExecuteAsync()
	{
		var context = await GetContextAsync().ConfigureAwait(false);
		if (context == null)
		{
			return;
		}

		context.Options = Options;
		context.Time = Time;
		await context.ExecuteAsync().ConfigureAwait(false);
	}

	private async Task<PunishmentBase?> GetContextAsync()
	{
		switch (Type)
		{
			case PunishmentType.Ban:
				return new Ban(Guild, UserId, IsGive);

			case PunishmentType.Softban:
				return IsGive ? new SoftBan(Guild, UserId) : null;
		}

		var user = await Guild.GetUserAsync(UserId).ConfigureAwait(false);
		return Type switch
		{
			PunishmentType.Deafen => new Deafen(user, IsGive),
			PunishmentType.VoiceMute => new Mute(user, IsGive),
			PunishmentType.Kick => IsGive ? new Kick(user) : null,
			PunishmentType.RoleMute when Role != null => new RoleMute(user, IsGive, Role),
			_ => null,
		};
	}
}