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
			public static async Task RoleMuteUser(IGuildUser user, IRole role, uint time = 0, ITimersModule timers = null)
			{
				await RoleActions.GiveRole(user, role);

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableRoleMute(user.Guild, user, time, role));
				}
			}
			public static async Task VoiceMuteUser(IGuildUser user, uint time = 0, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Mute = true);

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableVoiceMute(user.Guild, user, time));
				}
			}
			public static async Task DeafenUser(IGuildUser user, uint time = 0, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Deaf = true);

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableDeafen(user.Guild, user, time));
				}
			}

			public static async Task ManualRoleUnmuteUser(IGuildUser user, IRole role, ITimersModule timers = null)
			{
				await RoleActions.TakeRole(user, role);

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.RoleMute);
				}
			}
			public static async Task ManualVoiceUnmuteUser(IGuildUser user, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Mute = false);

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.VoiceMute);
				}
			}
			public static async Task ManualUndeafenUser(IGuildUser user, ITimersModule timers = null)
			{
				await user.ModifyAsync(x => x.Deaf = false);

				if (timers != null)
				{
					timers.RemovePunishments(user.Id, PunishmentType.Deafen);
				}
			}
			public static async Task ManualBan(ICommandContext context, ulong userID, int days = 0, uint time = 0, string reason = null, ITimersModule timers = null)
			{
				await context.Guild.AddBanAsync(userID, days, FormattingActions.FormatUserReason(context.User, reason));

				if (time > 0 && timers != null)
				{
					timers.AddRemovablePunishments(new RemovableBan(context.Guild, userID, time));
				}
			}
			public static async Task ManualSoftban(ICommandContext context, ulong userID, string reason = null)
			{
				await context.Guild.AddBanAsync(userID, 7, FormattingActions.FormatUserReason(context.User, reason));
				await context.Guild.RemoveBanAsync(userID);
			}
			public static async Task ManualKick(ICommandContext context, IGuildUser user, string reason = null)
			{
				await user.KickAsync(FormattingActions.FormatUserReason(context.User, reason));
			}

			public static async Task AutomaticRoleUnmuteUser(IGuildUser user, IRole role)
			{
				await RoleActions.TakeRole(user, role);
			}
			public static async Task AutomaticVoiceUnmuteUser(IGuildUser user)
			{
				await user.ModifyAsync(x => x.Mute = false);
			}
			public static async Task AutomaticUndeafenUser(IGuildUser user)
			{
				await user.ModifyAsync(x => x.Deaf = false);
			}
			public static async Task AutomaticBan(IGuild guild, ulong userID, [CallerMemberName] string reason = null)
			{
				await guild.AddBanAsync(userID, 7, FormattingActions.FormatBotReason(reason));
			}
			public static async Task AutomaticSoftban(IGuild guild, ulong userID, [CallerMemberName] string reason = null)
			{
				await guild.AddBanAsync(userID, 7, FormattingActions.FormatBotReason(reason));
				await guild.RemoveBanAsync(userID);
			}
			public static async Task AutomaticKick(IGuildUser user, [CallerMemberName] string reason = null)
			{
				await user.KickAsync(FormattingActions.FormatBotReason(reason));
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
					case PunishmentType.Nothing:
					{
						return;
					}
					case PunishmentType.Deafen:
					{
						await DeafenUser(user, time, timers);
						return;
					}
					case PunishmentType.VoiceMute:
					{
						await VoiceMuteUser(user, time, timers);
						return;
					}
					case PunishmentType.RoleMute:
					{
						await RoleMuteUser(user, guildSettings.MuteRole, time, timers);
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
				}
			}
		}
	}
}