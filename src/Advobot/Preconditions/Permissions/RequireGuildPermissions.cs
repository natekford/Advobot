using Discord;
using Discord.Commands;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoking user's permissions on a guild.
/// </summary>
/// <remarks>
/// Admin will always be added to the list of valid permissions.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireGuildPermissions(params GuildPermission[] permissions) : RequirePermissions(permissions.Cast<Enum>().Append(_Admin))
{
	private static readonly Enum _Admin = GuildPermission.Administrator;

	/// <inheritdoc />
	public override Task<Enum?> GetUserPermissionsAsync(
		ICommandContext context,
		IGuildUser user,
		IServiceProvider services)
	{
		var bits = user.GuildPermissions.RawValue;
		/* TODO: reimplement bot perms?
		if (!user.IsBot)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var match = settings.BotUsers.FirstOrDefault(x => x.UserId == context.User.Id);
			bits |= match?.Permissions ?? 0;
		}*/
		return Task.FromResult(bits == 0 ? null : (Enum)(GuildPermission)bits);
	}
}