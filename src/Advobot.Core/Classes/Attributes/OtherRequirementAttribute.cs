using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
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
		/// Initializes the attribute.
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
			if (!(context is AdvobotSocketCommandContext advobotCommandContext && context.User is SocketGuildUser user))
			{
				return PreconditionResult.FromError((string)null);
			}

			var permissions = (Requirements & Precondition.GenericPerms) != 0;
			var guildOwner = (Requirements & Precondition.GuildOwner) != 0;
			var trustedUser = (Requirements & Precondition.TrustedUser) != 0;
			var botOwner = (Requirements & Precondition.BotOwner) != 0;

			if (permissions)
			{
				var guildBits = user.GuildPermissions.RawValue;
				var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;

				var userPerms = guildBits | botBits;
				if ((userPerms & (ulong)USER_HAS_A_PERMISSION_PERMS) != 0)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			if (guildOwner && advobotCommandContext.Guild.OwnerId == user.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			if (trustedUser && advobotCommandContext.BotSettings.TrustedUsers.Contains(user.Id))
			{
				return PreconditionResult.FromSuccess();
			}
			if (botOwner && (await ClientUtils.GetBotOwnerAsync(advobotCommandContext.Client).CAF()).Id == user.Id)
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
