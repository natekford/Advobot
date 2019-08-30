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
			GuildPermission.Administrator,
			GuildPermission.BanMembers,
			GuildPermission.DeafenMembers,
			GuildPermission.KickMembers,
			GuildPermission.ManageChannels,
			GuildPermission.ManageEmojis,
			GuildPermission.ManageGuild,
			GuildPermission.ManageMessages,
			GuildPermission.ManageNicknames,
			GuildPermission.ManageRoles,
			GuildPermission.ManageWebhooks,
			GuildPermission.MoveMembers,
			GuildPermission.MuteMembers,
		};

		/// <summary>
		/// Creates an instance of <see cref="RequireGenericGuildPermissionsAttribute"/>.
		/// </summary>
		public RequireGenericGuildPermissionsAttribute() : base(_GenericPerms) { }

		/// <inheritdoc />
		public override string ToString()
			=> "Administrator | Any ending with 'Members' | Any starting with 'Manage'";
	}
}