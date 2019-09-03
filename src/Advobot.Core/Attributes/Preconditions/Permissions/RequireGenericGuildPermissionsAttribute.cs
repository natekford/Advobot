using System;

using Discord;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the invoker user has permissions which would allow them to use most commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireGenericGuildPermissionsAttribute : RequireGuildPermissionsAttribute
	{
		private static readonly GuildPermission[] _GenericPerms = new[]
		{
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
			GuildPermission.ManageEmojis,
		};

		/// <inheritdoc />
		public override string Summary
			=> "Administrator | Any ending with 'Members' | Any starting with 'Manage'";

		/// <summary>
		/// Creates an instance of <see cref="RequireGenericGuildPermissionsAttribute"/>.
		/// </summary>
		public RequireGenericGuildPermissionsAttribute() : base(_GenericPerms) { }
	}
}