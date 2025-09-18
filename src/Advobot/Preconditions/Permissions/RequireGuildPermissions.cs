using Advobot.Modules;

using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoking user's permissions on a guild.
/// </summary>
/// <remarks>
/// Admin will always be added to the list of valid permissions.
/// </remarks>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class RequireGuildPermissions(params GuildPermission[] permissions)
	: RequirePermissions(permissions.Cast<Enum>().Append(_Admin))
{
	private static readonly Enum _Admin = GuildPermission.Administrator;

	/// <inheritdoc />
	public override Task<Enum?> GetUserPermissionsAsync(
		IGuildContext context,
		IGuildUser user)
	{
		var bits = user.GuildPermissions.RawValue;
		return Task.FromResult(bits == 0 ? null : (Enum)(GuildPermission)bits);
	}
}