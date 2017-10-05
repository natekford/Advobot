using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	internal sealed class AutomaticPunishmentGiver : PunishmentGiver
	{
		internal AutomaticPunishmentGiver(int time, ITimersService timers) : base(time, timers) { }

		internal async Task AutomaticallyPunishAsync(PunishmentType punishmentType, IUser user, IRole role, [CallerMemberName] string reason = "")
			=> await AutomaticallyPunishAsync(punishmentType, user as IGuildUser, role, reason);
		internal async Task AutomaticallyPunishAsync(PunishmentType punishmentType, IGuildUser user, IRole role, [CallerMemberName] string reason = "")
		{
			//TODO: Rework the 4 big punishment things
			var guild = user.GetGuild();
			var bot = UserActions.GetBot(guild);
			if (!user.CanBeModifiedByUser(bot))
			{
				return;
			}

			var autoModReason = new AutomaticModerationReason(reason);
			switch (punishmentType)
			{
				case PunishmentType.Kick:
				{
					await KickAsync(user, autoModReason);
					return;
				}
				case PunishmentType.Ban:
				{
					await BanAsync(guild, user.Id, autoModReason);
					return;
				}
				case PunishmentType.Deafen:
				{
					await DeafenAsync(user, autoModReason);
					return;
				}
				case PunishmentType.VoiceMute:
				{
					await VoiceMuteAsync(user, autoModReason);
					return;
				}
				case PunishmentType.Softban:
				{
					await SoftbanAsync(guild, user.Id, autoModReason);
					return;
				}
				case PunishmentType.RoleMute:
				{
					await RoleMuteAsync(user, role, autoModReason);
					return;
				}
			}
		}
	}
}
