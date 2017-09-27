using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class Punishments
	{
		private static ITimersModule _Timers;
		private static bool _Installed;

		internal static void Install(IServiceProvider provider)
		{
			_Timers = provider.GetService<ITimersModule>();
			_Installed = _Timers != null;
		}

		/// <summary>
		/// Bans the user from the guild for the given amount of days. 
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <param name="days"></param>
		/// <param name="time"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public static async Task<IBan> Ban(IGuild guild, ulong userId, string reason, int days = 1, uint time = 0)
		{
			await guild.AddBanAsync(userId, days, reason);

			if (_Installed && time > 0)
			{
				_Timers.AddRemovablePunishments(new RemovablePunishment(PunishmentType.Ban, guild, userId, time));
			}

			return (await guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == userId);
		}
		public static async Task<IBan> Softban(IGuild guild, ulong userId, string reason)
		{
			await guild.AddBanAsync(userId, 7, reason);
			var ban = (await guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId);
			return ban;
		}
		public static async Task Kick(IGuildUser user, string reason)
		{
			await user.KickAsync(reason);
		}
		public static async Task RoleMute(IGuildUser user, IRole role, string reason, uint time = 0)
		{
			await RoleActions.GiveRoles(user, new[] { role }, reason);

			if (_Installed && time > 0)
			{
				_Timers.AddRemovablePunishments(new RemovablePunishment(PunishmentType.RoleMute, user.Guild, role, user.Id, time));
			}
		}
		public static async Task VoiceMute(IGuildUser user, string reason, uint time = 0)
		{
			await user.ModifyAsync(x => x.Mute = true, new RequestOptions { AuditLogReason = reason });

			if (_Installed && time > 0)
			{
				_Timers.AddRemovablePunishments(new RemovablePunishment(PunishmentType.VoiceMute, user.Guild, user.Id, time));
			}
		}
		public static async Task Deafen(IGuildUser user, string reason, uint time = 0)
		{
			await user.ModifyAsync(x => x.Deaf = true, new RequestOptions { AuditLogReason = reason });

			if (_Installed && time > 0)
			{
				_Timers.AddRemovablePunishments(new RemovablePunishment(PunishmentType.Deafen, user.Guild, user.Id, time));
			}
		}

		public static async Task Unban(IGuild guild, ulong userId, string reason)
		{
			await guild.RemoveBanAsync(userId, new RequestOptions { AuditLogReason = reason });

			if (_Installed)
			{
				_Timers.RemovePunishments(userId, PunishmentType.Ban);
			}
		}
		public static async Task RoleUnmute(IGuildUser user, IRole role, string reason)
		{
			await RoleActions.TakeRoles(user, new[] { role }, reason);

			if (_Installed)
			{
				_Timers.RemovePunishments(user.Id, PunishmentType.RoleMute);
			}
		}
		public static async Task VoiceUnmute(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Mute = false, new RequestOptions { AuditLogReason = reason });

			if (_Installed)
			{
				_Timers.RemovePunishments(user.Id, PunishmentType.VoiceMute);
			}
		}
		public static async Task Undeafen(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Deaf = false, new RequestOptions { AuditLogReason = reason });

			if (_Installed)
			{
				_Timers.RemovePunishments(user.Id, PunishmentType.Deafen);
			}
		}

		internal static async Task AutomaticPunishments(PunishmentType punishmentType, IGuildUser user, IRole role = null,
			bool alreadyKicked = false, uint time = 0, [CallerMemberName] string reason = "")
		{
			//TODO: Rework the 4 big punishment things
			//Basically a consolidation of 4 separate big banning things into one. I still need to rework a lot of this.
			var guild = user.Guild;
			if (!user.CanBeModifiedByUser(UserActions.GetBot(guild)))
			{
				return;
			}

			switch (punishmentType)
			{
				case PunishmentType.Deafen:
				{
					await Deafen(user, GeneralFormatting.FormatBotReason(reason));
					break;
				}
				case PunishmentType.VoiceMute:
				{
					await VoiceMute(user, GeneralFormatting.FormatBotReason(reason));
					break;
				}
				case PunishmentType.RoleMute:
				{
					await RoleMute(user, role, GeneralFormatting.FormatBotReason(reason));
					break;
				}
				case PunishmentType.Kick:
				{
					await Kick(user, GeneralFormatting.FormatBotReason(reason));
					return;
				}
				case PunishmentType.KickThenBan:
				{
					if (!alreadyKicked)
					{
						await Kick(user, GeneralFormatting.FormatBotReason(reason));
					}
					else
					{
						await Ban(guild, user.Id, GeneralFormatting.FormatBotReason(reason));
					}
					break;
				}
				case PunishmentType.Ban:
				{
					await Ban(guild, user.Id, GeneralFormatting.FormatBotReason(reason));
					break;
				}
				default:
				{
					return;
				}
			}

			if (_Installed && time > 0)
			{
				_Timers.AddRemovablePunishments(new RemovablePunishment(punishmentType, guild, role, user.Id, time));
			}
		}
	}
}