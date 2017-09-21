using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Permissions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class RoleActions
	{
		public static FailureReason VerifyRoleMeetsRequirements<T>(ICommandContext context, RoleVerification[] checkingTypes, T role) where T : IRole
		{
			if (role == null)
			{
				return FailureReason.TooFew;
			}

			var invokingUser = context.User as IGuildUser;
			var bot = UserActions.GetBot(context.Guild);
			foreach (var type in checkingTypes)
			{
				if (!invokingUser.GetIfUserCanDoActionOnRole(role, type))
				{
					return FailureReason.UserInability;
				}
				else if (!bot.GetIfUserCanDoActionOnRole(role, type))
				{
					return FailureReason.BotInability;
				}

				switch (type)
				{
					case RoleVerification.IsEveryone:
					{
						if (context.Guild.EveryoneRole.Id == role.Id)
						{
							return FailureReason.EveryoneRole;
						}
						break;
					}
					case RoleVerification.IsManaged:
					{
						if (role.IsManaged)
						{
							return FailureReason.ManagedRole;
						}
						break;
					}
				}
			}

			return FailureReason.NotFailure;
		}

		public static async Task<IEnumerable<string>> ModifyRolePermissions(IRole role, ActionType actionType, ulong changeValue, IGuildUser user)
		{
			//Only modify permissions the user has the ability to
			changeValue &= user.GuildPermissions.RawValue;

			var roleBits = role.Permissions.RawValue;
			switch (actionType)
			{
				case ActionType.Allow:
				{
					roleBits |= changeValue;
					break;
				}
				case ActionType.Deny:
				{
					roleBits &= ~changeValue;
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid ActionType provided.");
				}
			}

			await ModifyRolePermissions(role, roleBits, FormattingActions.FormatUserReason(user));
			return GuildPerms.ConvertValueToNames(changeValue);
		}
		public static async Task<int> ModifyRolePosition(IRole role, int position, string reason)
		{
			if (role == null)
			{
				return -1;
			}

			var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < UserActions.GetBot(role.Guild).GetPosition()).OrderBy(x => x.Position).ToArray();
			position = Math.Max(1, Math.Min(position, roles.Length));

			var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
			for (int i = 0; i < reorderProperties.Length; ++i)
			{
				if (i > position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i - 1].Id, i);
				}
				else if (i < position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i].Id, i);
				}
				else
				{
					reorderProperties[i] = new ReorderRoleProperties(role.Id, i);
				}
			}

			await role.Guild.ReorderRolesAsync(reorderProperties, new RequestOptions { AuditLogReason = reason, });
			return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
		}
		public static async Task ModifyRolePermissions(IRole role, ulong permissions, string reason)
		{
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), new RequestOptions { AuditLogReason = reason, });
		}
		public static async Task ModifyRoleName(IRole role, string name, string reason)
		{
			await role.ModifyAsync(x => x.Name = name, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyRoleColor(IRole role, Color color, string reason)
		{
			await role.ModifyAsync(x => x.Color = color, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyRoleHoist(IRole role, string reason)
		{
			await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task ModifyRoleMentionability(IRole role, string reason)
		{
			await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, new RequestOptions { AuditLogReason = reason });
		}

		public static async Task<IRole> GetMuteRole(ICommandContext context, IGuildSettings guildSettings)
		{
			var muteRole = guildSettings.MuteRole;
			var result = VerifyRoleMeetsRequirements(context, new[] { RoleVerification.CanBeEdited, RoleVerification.IsManaged }, muteRole);
			if (result != FailureReason.NotFailure)
			{
				muteRole = await context.Guild.CreateRoleAsync(Constants.MUTE_ROLE_NAME, new GuildPermissions(0));
				guildSettings.MuteRole = muteRole;
				guildSettings.SaveSettings();
			}

			const uint TEXT_PERMS = 0
				| (1U << (int)ChannelPermission.CreateInstantInvite)
				| (1U << (int)ChannelPermission.ManageChannel)
				| (1U << (int)ChannelPermission.ManagePermissions)
				| (1U << (int)ChannelPermission.ManageWebhooks)
				| (1U << (int)ChannelPermission.SendMessages)
				| (1U << (int)ChannelPermission.ManageMessages)
				| (1U << (int)ChannelPermission.AddReactions);
			foreach (var textChannel in await context.Guild.GetTextChannelsAsync())
			{
				if (textChannel.GetPermissionOverwrite(muteRole) == null)
				{
					await textChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, TEXT_PERMS));
				}
			}

			const uint VOICE_PERMS = 0
				| (1U << (int)ChannelPermission.CreateInstantInvite)
				| (1U << (int)ChannelPermission.ManageChannel)
				| (1U << (int)ChannelPermission.ManagePermissions)
				| (1U << (int)ChannelPermission.ManageWebhooks)
				| (1U << (int)ChannelPermission.Speak)
				| (1U << (int)ChannelPermission.MuteMembers)
				| (1U << (int)ChannelPermission.DeafenMembers)
				| (1U << (int)ChannelPermission.MoveMembers);
			foreach (var voiceChannel in await context.Guild.GetVoiceChannelsAsync())
			{
				if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
				{
					await voiceChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, VOICE_PERMS));
				}
			}

			return muteRole;
		}
		public static async Task<IRole> CreateRole(IGuild guild, string name, string reason)
		{
			return await guild.CreateRoleAsync(name, new GuildPermissions(0), options: new RequestOptions { AuditLogReason = reason, });
		}
		public static async Task DeleteRole(IRole role, string reason)
		{
			await role.DeleteAsync(new RequestOptions { AuditLogReason = reason, });
		}

		public static async Task GiveRoles(IGuildUser user, IEnumerable<IRole> roles, string reason)
		{
			await user.AddRolesAsync(roles, new RequestOptions { AuditLogReason = reason, });
		}
		public static async Task TakeRoles(IGuildUser user, IEnumerable<IRole> roles, string reason)
		{
			await user.RemoveRolesAsync(roles, new RequestOptions { AuditLogReason = reason, });
		}
	}
}