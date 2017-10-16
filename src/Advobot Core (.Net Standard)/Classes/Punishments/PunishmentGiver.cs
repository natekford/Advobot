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
	/// <summary>
	/// Gives a punishment to a user.
	/// </summary>
	public class PunishmentGiver : PunishmentBase
	{
		private int _Time;
		private bool _HasValidTimers;
		private ITimersService _Timers;
		private List<string> _Actions = new List<string>();

		public PunishmentGiver(int time, ITimersService timers)
		{
			_Time = time;
			_Timers = timers;
			_HasValidTimers = time > 0 && timers != null;
		}

		public async Task BanAsync(IGuild guild, ulong userId, ModerationReason reason, int days = 1)
		{
			await guild.AddBanAsync(userId, days, null, reason.CreateRequestOptions());
			var ban = (await guild.GetBansAsync()).Single(x => x.User.Id == userId);
			FollowupActions(PunishmentType.Ban, guild, ban.User, reason);
		}
		public async Task SoftbanAsync(IGuild guild, ulong userId, ModerationReason reason)
		{
			await guild.AddBanAsync(userId, 1, null, reason.CreateRequestOptions());
			var ban = (await guild.GetBansAsync()).Single(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.Softban, guild, ban.User, reason);
		}
		public async Task KickAsync(IGuildUser user, ModerationReason reason)
		{
			await user.KickAsync(null, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.Kick, user.Guild, user, reason);
		}
		public async Task RoleMuteAsync(IGuildUser user, IRole role, ModerationReason reason)
		{
			await RoleActions.GiveRolesAsync(user, new[] { role }, reason);
			FollowupActions(PunishmentType.RoleMute, user.Guild, user, reason);
		}
		public async Task VoiceMuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Mute = true, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.VoiceMute, user.Guild, user, reason);
		}
		public async Task DeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Deaf = true, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.Deafen, user.Guild, user, reason);
		}

		private void FollowupActions(PunishmentType punishmentType, IGuild guild, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Given[punishmentType]} {user.FormatUser()}. ");
			if (_HasValidTimers)
			{
				_Timers.AddRemovablePunishment(new RemovablePunishment(PunishmentType.Ban, guild, user, _Time));
				sb.Append($"They will be {_Removal[punishmentType]} in {_Time} minutes. ");
			}
			if (reason.HasReason)
			{
				sb.Append($"The provided reason is `{reason.Reason.EscapeBackTicks()}`. ");
			}
			_Actions.Add(sb.ToString());
		}

		public override string ToString()
		{
			return String.Join("\n", _Actions);
		}
	}
}
