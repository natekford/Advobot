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
	public class PunishmentGiver : PunishmentHandlerBase
	{
		public int Time				{ get; }
		public ITimersModule Timers { get; }
		public bool IsValid			{ get; }

		private List<string> _Actions = new List<string>();

		public PunishmentGiver(uint time, ITimersModule timers) : this((int)time, timers) { }
		public PunishmentGiver(int time, ITimersModule timers)
		{
			Time = time;
			Timers = timers;
			IsValid = time > 0 && timers != null;
		}

		public async Task BanAsync(IGuild guild, ulong userId, string reason, int days = 1)
		{
			await guild.AddBanAsync(userId, days, reason);
			var ban = (await guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == userId);
			FollowupActions(guild, ban.User, PunishmentType.Ban);
		}
		public async Task SoftbanAsync(IGuild guild, ulong userId, string reason)
		{
			await guild.AddBanAsync(userId, 7, reason);
			var ban = (await guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId);
			FollowupActions(guild, ban.User, PunishmentType.Softban);
		}
		public async Task KickAsync(IGuildUser user, string reason)
		{
			await user.KickAsync(reason);
			FollowupActions(user.Guild, user, PunishmentType.Kick);
		}
		public async Task RoleMuteAsync(IGuildUser user, IRole role, string reason)
		{
			await RoleActions.GiveRoles(user, new[] { role }, reason);
			FollowupActions(user.Guild, user, PunishmentType.RoleMute);
		}
		public async Task VoiceMuteAsync(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Mute = true, new RequestOptions { AuditLogReason = reason });
			FollowupActions(user.Guild, user, PunishmentType.VoiceMute);
		}
		public async Task DeafenAsync(IGuildUser user, string reason)
		{
			await user.ModifyAsync(x => x.Deaf = true, new RequestOptions { AuditLogReason = reason });
			FollowupActions(user.Guild, user, PunishmentType.Deafen);
		}

		private void FollowupActions(IGuild guild, IUser user, PunishmentType punishmentType)
		{
			var sb = new StringBuilder($"Successfully {_Given[punishmentType]} {user.FormatUser()}");
			if (IsValid)
			{
				Timers.AddRemovablePunishments(new RemovablePunishment(PunishmentType.Ban, guild, user.Id, Time));
				sb.Append($"and they will be {_Removal[punishmentType]} in {Time} minutes.");
			}
			_Actions.Add(sb.Append(".").ToString());
		}

		public override string ToString()
		{
			return String.Join("\n", _Actions);
		}
	}
}
