using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
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

		/// <summary>
		/// Bans a user from the guild. <paramref name="days"/> specifies how many days worth of messages to delete.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <param name="days"></param>
		/// <returns></returns>
		public async Task BanAsync(IGuild guild, ulong userId, ModerationReason reason, int days = 1)
		{
			await guild.AddBanAsync(userId, days, null, reason.CreateRequestOptions()).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			FollowupActions(PunishmentType.Ban, guild, ban.User, reason);
		}
		/// <summary>
		/// Bans then unbans a user from the guild. Deletes 1 days worth of messages.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task SoftbanAsync(IGuild guild, ulong userId, ModerationReason reason)
		{
			await guild.AddBanAsync(userId, 1, null, reason.CreateRequestOptions()).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.Softban, guild, ban.User, reason);
		}
		/// <summary>
		/// Kicks a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task KickAsync(IGuildUser user, ModerationReason reason)
		{
			await user.KickAsync(null, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.Kick, user.Guild, user, reason);
		}
		/// <summary>
		/// Gives a user the mute role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task RoleMuteAsync(IGuildUser user, IRole role, ModerationReason reason)
		{
			await RoleActions.GiveRolesAsync(user, new[] { role }, reason).CAF();
			FollowupActions(PunishmentType.RoleMute, user.Guild, user, reason);
		}
		/// <summary>
		/// Mutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task VoiceMuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Mute = true, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.VoiceMute, user.Guild, user, reason);
		}
		/// <summary>
		/// Deafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task DeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Deaf = true, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.Deafen, user.Guild, user, reason);
		}

		private void FollowupActions(PunishmentType punishmentType, IGuild guild, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Given[punishmentType]} {user.FormatUser()}. ");
			if (_HasValidTimers)
			{
				//Removing the punishments via the timers in whatever time is set
				_Timers.AddRemovablePunishment(new RemovablePunishment(punishmentType, guild, user, _Time));
				sb.Append($"They will be {_Removal[punishmentType]} in {_Time} minutes. ");
			}
			if (reason.HasReason)
			{
				sb.Append($"The provided reason is `{reason.Reason.EscapeBackTicks()}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}

		public override string ToString() => String.Join("\n", _Actions);
	}
}
