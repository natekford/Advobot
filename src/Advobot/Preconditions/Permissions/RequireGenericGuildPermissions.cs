using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.Preconditions.Permissions;

/// <summary>
/// Verifies the invoker user has permissions which would allow them to use most commands.
/// </summary>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public sealed class RequireGenericGuildPermissions : RequireGuildPermissions
{
	private static readonly GuildPermission[] _GenericPerms =
	[
		GuildPermission.Administrator,
		GuildPermission.BanMembers,
		GuildPermission.CreateEvents,
		GuildPermission.CreateGuildExpressions,
		GuildPermission.DeafenMembers,
		GuildPermission.KickMembers,
		GuildPermission.ManageChannels,
		GuildPermission.ManageEmojisAndStickers,
		GuildPermission.ManageEvents,
		GuildPermission.ManageGuild,
		GuildPermission.ManageMessages,
		GuildPermission.ManageNicknames,
		GuildPermission.ManageRoles,
		GuildPermission.ManageThreads,
		GuildPermission.ManageWebhooks,
		GuildPermission.MoveMembers,
		GuildPermission.MuteMembers,
	];

	/// <inheritdoc />
	public override string Summary
		=> "Administrator | Any ending with 'Members' | Any starting with 'Manage'";

	/// <summary>
	/// Creates an instance of <see cref="RequireGenericGuildPermissions"/>.
	/// </summary>
	public RequireGenericGuildPermissions() : base(_GenericPerms) { }
}