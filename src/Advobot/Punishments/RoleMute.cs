using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Mutes a user via a role.
/// </summary>
public sealed class RoleMute : GuildUserPunishmentBase
{
	/// <summary>
	/// Creates an instance of <see cref="RoleMute"/>.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="isGive"></param>
	/// <param name="role"></param>
	public RoleMute(IGuildUser user, bool isGive, IRole role) : base(user, isGive, PunishmentType.RoleMute)
	{
		Role = role;
	}

	/// <inheritdoc/>
	public override Task ExecuteAsync()
	{
		if (IsGive)
		{
			return User.AddRoleAsync(Role!, Options);
		}
		return User.RemoveRoleAsync(Role!, Options);
	}
}