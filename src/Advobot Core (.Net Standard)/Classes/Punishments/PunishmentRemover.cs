using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	public class PunishmentRemover : PunishmentHandlerBase
	{
		public ITimersModule Timers { get; }
		public bool IsValid			{ get; }

		private List<string> _Actions = new List<string>();

		public PunishmentRemover(ITimersModule timers)
		{
			Timers = timers;
			IsValid = timers != null;
		}

		public async Task UnbanAsync(IGuild guild, ulong userId, string reason)
		{
			var ban = (await guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, new RequestOptions { AuditLogReason = reason });
			FollowupActions(ban.User, PunishmentType.Ban);
		}
		public async Task RoleUnmuteAsync(IGuildUser user, IRole role, string reason)
		{
			await RoleActions.TakeRoles(user, new[] { role }, reason);
			FollowupActions(user, PunishmentType.RoleMute);
		}
		public async Task VoiceUnmuteAsync(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Mute = false, new RequestOptions { AuditLogReason = reason });
			FollowupActions(user, PunishmentType.VoiceMute);
		}
		public async Task UndeafenAsync(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Deaf = false, new RequestOptions { AuditLogReason = reason });
			FollowupActions(user, PunishmentType.Deafen);
		}

		private void FollowupActions(IUser user, PunishmentType punishmentType)
		{
			var sb = new StringBuilder($"Successfully {_Removal[punishmentType]} {user.FormatUser()}");
			if (IsValid && Timers.RemovePunishments(user.Id, punishmentType) > 0)
			{
				sb.Append($"and removed all timed {punishmentType.EnumName().FormatTitle().ToLower()} punishments on them");
			}
			_Actions.Add(sb.Append(".").ToString());
		}

		public override string ToString()
		{
			return String.Join("\n", _Actions);
		}
	}
}
