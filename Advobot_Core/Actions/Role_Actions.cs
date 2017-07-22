using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class RoleActions
		{
			public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, bool mentions, string input)
			{
				IRole role = null;
				if (!String.IsNullOrWhiteSpace(input))
				{
					if (ulong.TryParse(input, out ulong roleID))
					{
						role = GetRole(context.Guild, roleID);
					}
					else if (MentionUtils.TryParseRole(input, out roleID))
					{
						role = GetRole(context.Guild, roleID);
					}
					else
					{
						var roles = context.Guild.Roles.Where(x => x.Name.CaseInsEquals(input));
						if (roles.Count() == 1)
						{
							role = roles.First();
						}
						else if (roles.Count() > 1)
						{
							return new ReturnedObject<IRole>(role, FailureReason.TooMany);
						}
					}
				}

				if (role == null && mentions)
				{
					var roleMentions = context.Message.MentionedRoleIds;
					if (roleMentions.Count() == 1)
					{
						role = GetRole(context.Guild, roleMentions.First());
					}
					else if (roleMentions.Count() > 1)
					{
						return new ReturnedObject<IRole>(role, FailureReason.TooMany);
					}
				}

				return GetRole(context, checkingTypes, role);
			}
			public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
			{
				return GetRole(context, checkingTypes, GetRole(context.Guild, inputID));
			}
			public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, IRole role)
			{
				return GetRole(context.Guild, context.User as IGuildUser, checkingTypes, role);
			}
			public static ReturnedObject<T> GetRole<T>(IGuild guild, IGuildUser currUser, ObjectVerification[] checkingTypes, T role) where T : IRole
			{
				checkingTypes.AssertEnumsAreAllCorrectTargetType(role);
				if (role == null)
				{
					return new ReturnedObject<T>(role, FailureReason.TooFew);
				}

				var bot = UserActions.GetBot(guild);
				foreach (var type in checkingTypes)
				{
					if (!GetIfUserCanDoActionOnRole(role, currUser, type))
					{
						return new ReturnedObject<T>(role, FailureReason.UserInability);
					}
					else if (!GetIfUserCanDoActionOnRole(role, bot, type))
					{
						return new ReturnedObject<T>(role, FailureReason.BotInability);
					}

					switch (type)
					{
						case ObjectVerification.IsEveryone:
						{
							if (guild.EveryoneRole.Id == role.Id)
							{
								return new ReturnedObject<T>(role, FailureReason.EveryoneRole);
							}
							break;
						}
						case ObjectVerification.IsManaged:
						{
							if (role.IsManaged)
							{
								return new ReturnedObject<T>(role, FailureReason.ManagedRole);
							}
							break;
						}
					}
				}

				return new ReturnedObject<T>(role, FailureReason.NotFailure);
			}
			public static IRole GetRole(IGuild guild, ulong ID)
			{
				return guild.GetRole(ID);
			}
			public static bool GetIfUserCanDoActionOnRole(IRole target, IGuildUser user, ObjectVerification type)
			{
				if (target == null || user == null)
					return false;

				switch (type)
				{
					case ObjectVerification.CanBeEdited:
					{
						return target.Position < UserActions.GetUserPosition(user);
					}
					default:
					{
						return true;
					}
				}
			}

			public static async Task<int> ModifyRolePosition(IRole role, int position)
			{
				if (role == null)
					return -1;

				var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < UserActions.GetUserPosition(UserActions.GetBot(role.Guild))).OrderBy(x => x.Position).ToArray();
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

				await role.Guild.ReorderRolesAsync(reorderProperties);
				return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
			}

			public static async Task<IRole> GetMuteRole(IGuildSettings guildSettings, IGuild guild, IGuildUser user)
			{
				var returnedMuteRole = GetRole(guild, user, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsManaged }, guildSettings.MuteRole);
				var muteRole = returnedMuteRole.Object;
				if (muteRole == null)
				{
					muteRole = await guild.CreateRoleAsync(Constants.MUTE_ROLE_NAME, new GuildPermissions(0));
					//TODO: guildSettings.SetSetting(SettingOnGuild.MuteRole, new DiscordObjectWithId<IRole>(muteRole));
				}

				const uint TEXT_PERMS = 0
					| (1U << (int)ChannelPermission.CreateInstantInvite)
					| (1U << (int)ChannelPermission.ManageChannel)
					| (1U << (int)ChannelPermission.ManagePermissions)
					| (1U << (int)ChannelPermission.ManageWebhooks)
					| (1U << (int)ChannelPermission.SendMessages)
					| (1U << (int)ChannelPermission.ManageMessages)
					| (1U << (int)ChannelPermission.AddReactions);
				foreach (var textChannel in await guild.GetTextChannelsAsync())
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
				foreach (var voiceChannel in await guild.GetVoiceChannelsAsync())
				{
					if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await voiceChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, VOICE_PERMS));
					}
				}

				return muteRole;
			}

			public static async Task GiveRole(IGuildUser user, IRole role)
			{
				if (role == null)
					return;
				if (user.RoleIds.Contains(role.Id))
					return;
				await user.AddRoleAsync(role);
			}
			public static async Task GiveRoles(IGuildUser user, IEnumerable<IRole> roles)
			{
				if (!roles.Any())
					return;

				await user.AddRolesAsync(roles);
			}
			public static async Task TakeRole(IGuildUser user, IRole role)
			{
				if (role == null)
					return;
				if (!user.RoleIds.Contains(role.Id))
					return;
				await user.RemoveRoleAsync(role);
			}
			public static async Task TakeRoles(IGuildUser user, IEnumerable<IRole> roles)
			{
				if (!roles.Any())
					return;

				await user.RemoveRolesAsync(roles);
			}
		}
	}
}