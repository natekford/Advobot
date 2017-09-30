using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	internal sealed class AutomaticPunishmentGiver : PunishmentGiver
	{
		public AutomaticPunishmentGiver(uint time, ITimersModule timers) : base(time, timers) { }
		public AutomaticPunishmentGiver(int time, ITimersModule timers) : base(time, timers) { }

		internal async Task AutomaticallyPunishAsync(PunishmentType punishmentType, IGuildUser user, IRole role = null, [CallerMemberName] string reason = "")
		{
			//TODO: Rework the 4 big punishment things
			//Basically a consolidation of 4 separate big banning things into one. I still need to rework a lot of this.
			var guild = user.Guild;
			if (!user.CanBeModifiedByUser(UserActions.GetBot(guild)))
			{
				return;
			}

			var formattedReason = GeneralFormatting.FormatBotReason(reason);
			switch (punishmentType)
			{
				case PunishmentType.Deafen:
				{
					await DeafenAsync(user, formattedReason);
					break;
				}
				case PunishmentType.VoiceMute:
				{
					await VoiceMuteAsync(user, formattedReason);
					break;
				}
				case PunishmentType.RoleMute:
				{
					await RoleMuteAsync(user, role, formattedReason);
					break;
				}
				case PunishmentType.Kick:
				{
					await KickAsync(user, formattedReason);
					return;
				}
				case PunishmentType.Softban:
				{
					await SoftbanAsync(guild, user.Id, formattedReason);
					break;
				}
				case PunishmentType.Ban:
				{
					await BanAsync(guild, user.Id, formattedReason);
					break;
				}
				default:
				{
					return;
				}
			}
		}
	}
}
