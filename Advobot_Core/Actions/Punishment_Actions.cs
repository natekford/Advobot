using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.RemovablePunishments;
using Discord;
using Discord.Commands;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class PunishmentActions
		{
			public static async Task ManualBan(IGuild guild, ulong userId, string reason, int days = 1, uint time = 0, ITimersModule timers = null)
			{
				await guild.AddBanAsync(userId, days, reason);

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableBan(guild, userId, time));
				}
			}
			public static async Task ManualSoftban(IGuild guild, ulong userId, string reason)
			{
				await guild.AddBanAsync(userId, 7, reason);
				await guild.RemoveBanAsync(userId);
			}
			public static async Task ManualKick(IGuildUser user, string reason)
			{
				await user.KickAsync(reason);
			}
			public static async Task ManualRoleMuteUser(IGuildUser user, IRole role, string reason, uint time = 0, ITimersModule timers = null)
			{
				await RoleActions.GiveRoles(user, new[] { role }, reason);

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableRoleMute(user.Guild, user, time, role));
				}
			}
			public static async Task ManualVoiceMuteUser(IGuildUser user, string reason, uint time = 0, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Mute = true, new RequestOptions { AuditLogReason = reason });

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableVoiceMute(user.Guild, user, time));
				}
			}
			public static async Task ManualDeafenUser(IGuildUser user, string reason, uint time = 0, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Deaf = true, new RequestOptions { AuditLogReason = reason });

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableDeafen(user.Guild, user, time));
				}
			}

			public static async Task ManualUnbanUser(IGuild guild, ulong userId, string reason, ITimersModule timers = null)
			{
				await guild.RemoveBanAsync(userId, new RequestOptions { AuditLogReason = reason });

				if (timers != null)
				{
					timers.RemovePunishments(userId, PunishmentType.Ban);
				}
			}
			public static async Task ManualRoleUnmuteUser(IGuildUser user, IRole role, string reason, ITimersModule timers = null)
			{
				await RoleActions.TakeRoles(user, new[] { role }, reason);

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.RoleMute);
				}
			}
			public static async Task ManualVoiceUnmuteUser(IGuildUser user, string reason, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Mute = false, new RequestOptions { AuditLogReason = reason });

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.VoiceMute);
				}
			}
			public static async Task ManualUndeafenUser(IGuildUser user, string reason, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Deaf = false, new RequestOptions { AuditLogReason = reason });

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.Deafen);
				}
			}

			public static async Task AutomaticBan(IGuild guild, ulong userId, [CallerMemberName] string reason = null)
			{
				await ManualBan(guild, userId, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticSoftban(IGuild guild, ulong userId, [CallerMemberName] string reason = null)
			{
				await ManualSoftban(guild, userId, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticKick(IGuildUser user, [CallerMemberName] string reason = null)
			{
				await ManualKick(user, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticRoleMuteUser(IGuildUser user, IRole role, [CallerMemberName] string reason = null)
			{
				await ManualRoleMuteUser(user, role, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticVoiceMuteUser(IGuildUser user, [CallerMemberName] string reason = null)
			{
				await ManualVoiceMuteUser(user, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticDeafenUser(IGuildUser user, [CallerMemberName] string reason = null)
			{
				await ManualDeafenUser(user, FormattingActions.FormatBotReason(reason));
			}

			public static async Task AutomaticUnbanUser(IGuild guild, ulong userId)
			{
				await ManualUnbanUser(guild, userId, FormattingActions.FormatBotReason("automatic unban."));
			}
			public static async Task AutomaticRoleUnmuteUser(IGuildUser user, IRole role)
			{
				await ManualRoleUnmuteUser(user, role, FormattingActions.FormatBotReason("automatic role unmute."));
			}
			public static async Task AutomaticVoiceUnmuteUser(IGuildUser user)
			{
				await ManualVoiceUnmuteUser(user, FormattingActions.FormatBotReason("automatic voice unmute."));
			}
			public static async Task AutomaticUndeafenUser(IGuildUser user)
			{
				await ManualUndeafenUser(user, FormattingActions.FormatBotReason("automatic undeafen."));
			}

			public static async Task AutomaticPunishments(IGuildSettings guildSettings, IGuildUser user, PunishmentType punishmentType, bool alreadyKicked = false,
														  uint time = 0, ITimersModule timers = null, [CallerMemberName] string caller = "")
			{
				//TODO: Rework the 4 big punishment things
				//Basically a consolidation of 4 separate big banning things into one. I still need to rework a lot of this.
				var guild = user.Guild;
				if (!UserActions.GetIfUserCanBeModifiedByUser(UserActions.GetBot(guild), user))
					return;

				switch (punishmentType)
				{
					case PunishmentType.Deafen:
					{
						await AutomaticDeafenUser(user);
						if (time > 0 && timers != null)
						{
							timers.AddRemovablePunishments(new RemovableDeafen(guild, user, time));
						}
						return;
					}
					case PunishmentType.VoiceMute:
					{
						await AutomaticVoiceMuteUser(user);
						if (time > 0 && timers != null)
						{
							timers.AddRemovablePunishments(new RemovableVoiceMute(guild, user, time));
						}
						return;
					}
					case PunishmentType.RoleMute:
					{
						await AutomaticRoleMuteUser(user, guildSettings.MuteRole);
						if (time > 0 && timers != null)
						{
							timers.AddRemovablePunishments(new RemovableRoleMute(guild, user, time, guildSettings.MuteRole));
						}
						return;
					}
					case PunishmentType.Kick:
					{
						await AutomaticKick(user, caller);
						return;
					}
					case PunishmentType.KickThenBan:
					{
						if (!alreadyKicked)
						{
							await AutomaticKick(user, caller);
						}
						else
						{
							await AutomaticBan(guild, user.Id, caller);
							if (time > 0 && timers != null)
							{
								timers.AddRemovablePunishments(new RemovableBan(guild, user, time));
							}
						}
						return;
					}
					case PunishmentType.Ban:
					{
						await AutomaticBan(guild, user.Id, caller);
						if (time > 0 && timers != null)
						{
							timers.AddRemovablePunishments(new RemovableBan(guild, user, time));
						}
						return;
					}
					default:
					{
						return;
					}
				}
			}
		}
	}
}