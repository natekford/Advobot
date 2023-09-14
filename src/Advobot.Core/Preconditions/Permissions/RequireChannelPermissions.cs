using Discord;
using Discord.Commands;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoking user's permissions on the context channel.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireChannelPermissions(params ChannelPermission[] permissions) : RequirePermissions(permissions.Cast<Enum>())
{

	/// <inheritdoc />
	public override Task<Enum?> GetUserPermissionsAsync(
		ICommandContext context,
		IGuildUser user,
		IServiceProvider services)
	{
		if (context.Channel is not ITextChannel channel)
		{
			return Task.FromResult<Enum?>(null);
		}

		var bits = user.GetPermissions(channel).RawValue;
		var e = bits == 0 ? null : (Enum)(ChannelPermission)bits;
		return Task.FromResult(e);
	}
}