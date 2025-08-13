using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Mutes a user via a role.
/// </summary>
/// <param name="user"></param>
/// <param name="role"></param>
/// <param name="isGive"></param>
public sealed class RoleMute(IGuildUser user, IRole role, bool isGive)
	: PunishmentBase(user.Guild, user.Id, isGive)
{
	/// <inheritdoc cref="IPunishment.RoleId" />
	public IRole Role
	{
		get;
		init
		{
			field = value;
			RoleId = value?.Id ?? 0;
		}
	} = role;
	/// <inheritdoc />
	public override PunishmentType Type => PunishmentType.RoleMute;
	/// <inheritdoc cref="IPunishment.UserId" />
	public IGuildUser User { get; } = user;

	/// <inheritdoc/>
	public override Task ExecuteAsync(RequestOptions? options = null)
	{
		if (IsGive)
		{
			return User.AddRoleAsync(Role, options);
		}
		return User.RemoveRoleAsync(Role, options);
	}
}