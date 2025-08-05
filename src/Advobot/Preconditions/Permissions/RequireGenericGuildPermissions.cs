using Discord;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoker user has permissions which would allow them to use most commands.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireGenericGuildPermissions : RequireGuildPermissions
{
	private static readonly GuildPermission[] _GenericPerms =
	[
		GuildPermission.KickMembers,
		GuildPermission.BanMembers,
		GuildPermission.Administrator,
		GuildPermission.ManageChannels,
		GuildPermission.ManageGuild,
		GuildPermission.ManageMessages,
		GuildPermission.MuteMembers,
		GuildPermission.DeafenMembers,
		GuildPermission.MoveMembers,
		GuildPermission.ManageNicknames,
		GuildPermission.ManageRoles,
		GuildPermission.ManageWebhooks,
		GuildPermission.ManageEmojisAndStickers,
	];

	/// <inheritdoc />
	public override string Summary
		=> "Administrator | Any ending with 'Members' | Any starting with 'Manage'";

	/// <summary>
	/// Creates an instance of <see cref="RequireGenericGuildPermissions"/>.
	/// </summary>
	public RequireGenericGuildPermissions() : base(_GenericPerms) { }
}