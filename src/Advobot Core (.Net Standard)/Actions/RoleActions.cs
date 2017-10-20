using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Permissions;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Interfaces;
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
		/// <summary>
		/// Verifies that the role can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult VerifyRoleMeetsRequirements(this IRole target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(target, CommandError.ObjectNotFound, "Unable to find a matching role.");
			}

			var invokingUser = context.User as IGuildUser;
			var bot = context.Guild.GetBot();
			foreach (var check in checks)
			{
				if (!UserActions.GetIfUserCanDoActionOnRole(invokingUser, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the role: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
				else if (!UserActions.GetIfUserCanDoActionOnRole(bot, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the role: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}

				switch (check)
				{
					case ObjectVerification.IsEveryone:
					{
						if (context.Guild.EveryoneRole.Id != target.Id)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition, "The everyone role cannot be modified in that way.");
						}
						break;
					}
					case ObjectVerification.IsManaged:
					{
						if (!target.IsManaged)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition, "Managed roles cannot be modified in that way.");
						}
						break;
					}
				}
			}

			return new VerifiedObjectResult(target, null, null);
		}

		/// <summary>
		/// Makes sure the guild has a mute role, if not creates one. Also updates all the permisions on the channels so the mute
		/// role remains effective.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="guildSettings"></param>
		/// <returns></returns>
		public static async Task<IRole> GetMuteRoleAsync(ICommandContext context, IGuildSettings guildSettings)
		{
			var muteRole = guildSettings.MuteRole;
			if (!VerifyRoleMeetsRequirements(muteRole, context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsManaged }).IsSuccess)
			{
				muteRole = await context.Guild.CreateRoleAsync(Constants.MUTE_ROLE_NAME, new GuildPermissions(0)).CAF();
				guildSettings.MuteRole = muteRole;
				guildSettings.SaveSettings();
			}

			const ulong TEXT_PERMS = 0
				| (1UL << (int)ChannelPermission.CreateInstantInvite)
				| (1UL << (int)ChannelPermission.ManageChannel)
				| (1UL << (int)ChannelPermission.ManagePermissions)
				| (1UL << (int)ChannelPermission.ManageWebhooks)
				| (1UL << (int)ChannelPermission.SendMessages)
				| (1UL << (int)ChannelPermission.ManageMessages)
				| (1UL << (int)ChannelPermission.AddReactions);
			foreach (var textChannel in await context.Guild.GetTextChannelsAsync().CAF())
			{
				if (textChannel.GetPermissionOverwrite(muteRole) == null)
				{
					await textChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, TEXT_PERMS)).CAF();
				}
			}

			const ulong VOICE_PERMS = 0
				| (1UL << (int)ChannelPermission.CreateInstantInvite)
				| (1UL << (int)ChannelPermission.ManageChannel)
				| (1UL << (int)ChannelPermission.ManagePermissions)
				| (1UL << (int)ChannelPermission.ManageWebhooks)
				| (1UL << (int)ChannelPermission.Speak)
				| (1UL << (int)ChannelPermission.MuteMembers)
				| (1UL << (int)ChannelPermission.DeafenMembers)
				| (1UL << (int)ChannelPermission.MoveMembers);
			foreach (var voiceChannel in await context.Guild.GetVoiceChannelsAsync().CAF())
			{
				if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
				{
					await voiceChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, VOICE_PERMS)).CAF();
				}
			}

			return muteRole;
		}
		/// <summary>
		/// Creates a role then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<IRole> CreateRoleAsync(IGuild guild, string name, ModerationReason reason)
		{
			return await guild.CreateRoleAsync(name, new GuildPermissions(0), options: reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Deletes a a role then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task DeleteRoleAsync(IRole role, ModerationReason reason)
		{
			await role.DeleteAsync(reason.CreateRequestOptions()).CAF();
		}

		/// <summary>
		/// Gives the roles to a user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="roles"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task GiveRolesAsync(IGuildUser user, IEnumerable<IRole> roles, ModerationReason reason)
		{
			await user.AddRolesAsync(roles, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Removes the roles from a user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="roles"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task TakeRolesAsync(IGuildUser user, IEnumerable<IRole> roles, ModerationReason reason)
		{
			await user.RemoveRolesAsync(roles, reason.CreateRequestOptions()).CAF();
		}

		/// <summary>
		/// Changes the role's permissions by allowing or denying the supplied change value from them.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="permValue"></param>
		/// <param name="changeValue"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<string>> ModifyRolePermissionsAsync(IRole role, PermValue permValue, ulong changeValue, IGuildUser user)
		{
			var roleBits = role.Permissions.RawValue;
			switch (permValue)
			{
				//Only modify permissions the user has the ability to
				case PermValue.Allow:
				{
					roleBits |= (changeValue & user.GuildPermissions.RawValue);
					break;
				}
				case PermValue.Deny:
				{
					roleBits &= ~(changeValue & user.GuildPermissions.RawValue);
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid ActionType provided.");
				}
			}

			await ModifyRolePermissionsAsync(role, roleBits, new ModerationReason(user, null)).CAF();
			return GuildPerms.ConvertValueToNames(changeValue);
		}
		/// <summary>
		/// Changes the role's position and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="position"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> ModifyRolePositionAsync(IRole role, int position, ModerationReason reason)
		{
			if (role == null)
			{
				return -1;
			}

			var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < role.Guild.GetBot().GetPosition()).OrderBy(x => x.Position).ToArray();
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

			await role.Guild.ReorderRolesAsync(reorderProperties, reason.CreateRequestOptions()).CAF();
			return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
		}
		/// <summary>
		/// Changes the role's permissions and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="permissions"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyRolePermissionsAsync(IRole role, ulong permissions, ModerationReason reason)
		{
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Changes the role's name and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="name"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyRoleNameAsync(IRole role, string name, ModerationReason reason)
		{
			await role.ModifyAsync(x => x.Name = name, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Changes the role's color and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="color"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyRoleColorAsync(IRole role, Color color, ModerationReason reason)
		{
			await role.ModifyAsync(x => x.Color = color, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Changes the role's hoist status and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyRoleHoistAsync(IRole role, ModerationReason reason)
		{
			await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Changes the role's mentionability and says the the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyRoleMentionabilityAsync(IRole role, ModerationReason reason)
		{
			await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, reason.CreateRequestOptions()).CAF();
		}
	}
}