using Advobot.Modules;

using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoking user's permissions on the context channel.
/// </summary>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class RequireChannelPermissions(params ChannelPermission[] permissions)
	: RequirePermissions(permissions.Cast<Enum>())
{
	/// <inheritdoc />
	public override Task<Enum?> GetUserPermissionsAsync(
		IGuildContext context,
		IGuildUser user)
	{
		var bits = user.GetPermissions(context.Channel).RawValue;
		return Task.FromResult(bits == 0 ? null : (Enum)(ChannelPermission)bits);
	}
}