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
	/// Removes a punishment from a user.
	/// </summary>
	public class PunishmentRemover : PunishmentBase
	{
		private bool _HasValidTimers;
		private ITimersService _Timers;
		private List<string> _Actions = new List<string>();

		public PunishmentRemover(ITimersService timers)
		{
			this._Timers = timers;
			this._HasValidTimers = timers != null;
		}

		/// <summary>
		/// Removes a user from the ban list.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UnbanAsync(IGuild guild, ulong userId, ModerationReason reason)
		{
			var ban = (await guild.GetBansAsync().CAF()).SingleOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.Ban, ban.User, reason);
		}
		/// <summary>
		/// Removes the mute role from the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UnrolemuteAsync(IGuildUser user, IRole role, ModerationReason reason)
		{
			await RoleActions.TakeRolesAsync(user, new[] { role }, reason).CAF();
			FollowupActions(PunishmentType.RoleMute, user, reason);
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UnvoicemuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Mute = false, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.VoiceMute, user, reason);
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UndeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Deaf = false, reason.CreateRequestOptions()).CAF();
			FollowupActions(PunishmentType.Deafen, user, reason);
		}

		private void FollowupActions(PunishmentType punishmentType, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Removal[punishmentType]} {user.FormatUser()}. ");
			if (this._HasValidTimers && this._Timers.RemovePunishments(user.Id, punishmentType) > 0)
			{
				sb.Append($"Removed all timed {punishmentType.EnumName().FormatTitle().ToLower()} punishments on them. ");
			}
			if (reason.HasReason)
			{
				sb.Append($"The provided reason is `{reason.Reason.EscapeBackTicks()}`. ");
			}
			this._Actions.Add(sb.ToString().Trim());
		}

		public override string ToString() => String.Join("\n", this._Actions);
	}
}
