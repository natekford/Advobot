using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// For verifying <see cref="SocketGuildUser"/> permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class PermissionRequirementAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Indicates this user has a permission which should allow them to use basic commands which could potentially be spammy.
		/// </summary>
		public const GuildPermission GenericPerms = 0
			| GuildPermission.Administrator
			| GuildPermission.BanMembers
			| GuildPermission.DeafenMembers
			| GuildPermission.KickMembers
			| GuildPermission.ManageChannels
			| GuildPermission.ManageEmojis
			| GuildPermission.ManageGuild
			| GuildPermission.ManageMessages
			| GuildPermission.ManageNicknames
			| GuildPermission.ManageRoles
			| GuildPermission.ManageWebhooks
			| GuildPermission.MoveMembers
			| GuildPermission.MuteMembers;

		/// <summary>
		/// The flags required (each is a separate valid combination of flags).
		/// </summary>
		public ImmutableHashSet<GuildPermission> Permissions { get; }

		private readonly string _PermissionsText;

		/// <summary>
		/// Creates an instance of <see cref="PermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public PermissionRequirementAttribute(params GuildPermission[] permissions)
		{
			Permissions = permissions
				.Concat(new[] { GuildPermission.Administrator })
				.ToImmutableHashSet();

			var text = Permissions.FormatPermissions(x =>
			{
				//Special case, greatly shortens the output string while retaining what it means
				if (x == GenericPerms)
				{
					return "Any ending with 'Members' | Any starting with 'Manage'";
				}
				return null;
			});
			_PermissionsText = $"[{text}]";
		}

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var userPerms = await GetUserPermissionsAsync(context, services).CAF();
			//If the user has no permissions this should just return an error
			if (userPerms == 0)
			{
				return PreconditionResult.FromError("You have no permissions.");
			}

			foreach (ulong permission in Permissions)
			{
				if ((userPerms & permission) == permission)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError("You are missing permissions");
		}
		/// <summary>
		/// Returns the invoking user's permissions.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<ulong> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services);
		/// <inheritdoc />
		public override string ToString()
			=> _PermissionsText;
	}
}
