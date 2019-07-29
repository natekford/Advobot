using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
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
	public abstract class PermissionRequirementAttribute : AdvobotPreconditionAttribute
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
		public ImmutableArray<GuildPermissions> Permissions { get; }
		/// <summary>
		/// Text displaying all the valid permissions in a single string.
		/// </summary>
		public string PermissionsText { get; }

		/// <summary>
		/// Creates an instance of <see cref="PermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public PermissionRequirementAttribute(params GuildPermission[] permissions)
		{
			var list = permissions.ToList();
			if (!permissions.Contains(GuildPermission.Administrator))
			{
				list.Add(GuildPermission.Administrator);
			}

			Permissions = list.Select(x => new GuildPermissions((ulong)x)).Where(x => x.RawValue != 0).ToImmutableArray();
			var text = Permissions.Select(x =>
			{
				//Special case, greatly shortens the output string while retaining what it means
				if (x.RawValue == (ulong)GenericPerms)
				{
					return "Any ending with 'Members' | Any starting with 'Manage'";
				}

				var perms = new List<string>();
				foreach (GuildPermission e in Enum.GetValues(typeof(GuildPermission)))
				{
					if (x.RawValue == (ulong)e)
					{
						return e.ToString();
					}
					if (x.Has(e))
					{
						perms.Add(e.ToString());
					}
				}
				return perms.JoinNonNullStrings(" & ");
			});
			PermissionsText = $"[{string.Join(" | ", text)}]";
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			var userPerms = GetUserPermissions(context);
			//If the user has no permissions this should just return an error
			if (userPerms == 0)
			{
				return Task.FromResult(PreconditionResult.FromError("You have no permissions."));
			}

			foreach (var validPermissions in Permissions)
			{
				if ((userPerms & validPermissions.RawValue) == validPermissions.RawValue)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError("You are missing permissions"));
		}
		/// <summary>
		/// Returns the invoking user's permissions.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public abstract ulong GetUserPermissions(AdvobotCommandContext context);
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => PermissionsText;
	}
}
