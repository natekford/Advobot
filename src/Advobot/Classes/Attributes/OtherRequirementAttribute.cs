using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Checks if a user has any permissions that would generally be needed for a command, if the user is the guild owner, if the user if the bot owner, or if the user is a trusted user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class OtherRequirementAttribute : PreconditionAttribute
	{
		private const GuildPermission USER_HAS_A_PERMISSION_PERMS = 0
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
		/// Preconditions that need to be met before the command fires successfully.
		/// </summary>
		public Precondition Requirements { get; }

		/// <summary>
		/// Creates an instance of <see cref="OtherRequirementAttribute"/>.
		/// </summary>
		/// <param name="requirements"></param>
		public OtherRequirementAttribute(Precondition requirements)
		{
			Requirements = requirements;
		}

		/// <summary>
		/// Checks each precondition. If any fail, returns an error.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (!(context is AdvobotCommandContext aContext))
			{
				throw new ArgumentException("Invalid context provided.");
			}
			if (!(context.User is SocketGuildUser user))
			{
				return PreconditionResult.FromError("Unable to get the current user.");
			}
			if ((Requirements & Precondition.GenericPerms) != 0)
			{
				var guildBits = user.GuildPermissions.RawValue;
				var botBits = aContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
				if (((guildBits | botBits) & (ulong)USER_HAS_A_PERMISSION_PERMS) != 0)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			if ((Requirements & Precondition.GuildOwner) != 0 && aContext.Guild.OwnerId == user.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			if ((Requirements & Precondition.TrustedUser) != 0 && aContext.BotSettings.TrustedUsers.Contains(user.Id))
			{
				return PreconditionResult.FromSuccess();
			}
			if ((Requirements & Precondition.BotOwner) != 0 && await ClientUtils.GetOwnerIdAsync(aContext.Client).CAF() == user.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError((string)null);
		}

		/// <summary>
		/// Returns the preconditions in a readable format.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var text = new List<string>();
			if ((Requirements & Precondition.GenericPerms) != 0)
			{
				text.Add("Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'");
			}
			if ((Requirements & Precondition.GuildOwner) != 0)
			{
				text.Add("Guild Owner");
			}
			if ((Requirements & Precondition.TrustedUser) != 0)
			{
				text.Add("Trusted User");
			}
			if ((Requirements & Precondition.BotOwner) != 0)
			{
				text.Add("Bot Owner");
			}
			return $"[{String.Join(" | ", text)}]";
		}
	}
}
