using Advobot.Modules;

using Discord;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoking user's permissions on the context channel.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
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