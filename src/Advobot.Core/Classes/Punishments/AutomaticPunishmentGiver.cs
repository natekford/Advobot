using Advobot.Core.Actions;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Automatically gives a punishment to a user.
	/// </summary>
	public class AutomaticPunishmentGiver : PunishmentGiver
	{
		public AutomaticPunishmentGiver(int time, ITimersService timers) : base(time, timers) { }

		public virtual async Task AutomaticallyPunishAsync(PunishmentType punishmentType, IUser user, IRole role, [CallerMemberName] string reason = "")
			=> await AutomaticallyPunishAsync(punishmentType, user as IGuildUser, role, reason).CAF();
		public virtual async Task AutomaticallyPunishAsync(PunishmentType punishmentType, IGuildUser user, IRole role, [CallerMemberName] string reason = "")
		{
			var guild = user.GetGuild();
			var bot = guild.GetBot();
			if (!bot.GetIfCanModifyUser(user))
			{
				return;
			}

			var autoModReason = new AutomaticModerationReason(reason);
			switch (punishmentType)
			{
				case PunishmentType.Kick:
				{
					await KickAsync(user, autoModReason).CAF();
					return;
				}
				case PunishmentType.Ban:
				{
					await BanAsync(guild, user.Id, autoModReason).CAF();
					return;
				}
				case PunishmentType.Deafen:
				{
					await DeafenAsync(user, autoModReason).CAF();
					return;
				}
				case PunishmentType.VoiceMute:
				{
					await VoiceMuteAsync(user, autoModReason).CAF();
					return;
				}
				case PunishmentType.Softban:
				{
					await SoftbanAsync(guild, user.Id, autoModReason).CAF();
					return;
				}
				case PunishmentType.RoleMute:
				{
					await RoleMuteAsync(user, role, autoModReason).CAF();
					return;
				}
			}
		}
	}
}
