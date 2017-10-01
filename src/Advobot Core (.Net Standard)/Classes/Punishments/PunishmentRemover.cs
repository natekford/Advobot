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
	public class PunishmentRemover : PunishmentBase
	{
		public ITimersService Timers { get; }
		public bool IsValid { get; }

		private List<string> _Actions = new List<string>();

		public PunishmentRemover(ITimersService timers)
		{
			Timers = timers;
			IsValid = timers != null;
		}

		public async Task UnbanAsync(IGuild guild, ulong userId, ModerationReason reason)
		{
			var ban = (await guild.GetBansAsync()).SingleOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.Ban, ban.User, reason);
		}
		public async Task UnrolemuteAsync(IGuildUser user, IRole role, ModerationReason reason)
		{
			await RoleActions.TakeRoles(user, new[] { role }, reason);
			FollowupActions(PunishmentType.RoleMute, user, reason);
		}
		public async Task UnvoicemuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Mute = false, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.VoiceMute, user, reason);
		}
		public async Task UndeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Deaf = false, reason.CreateRequestOptions());
			FollowupActions(PunishmentType.Deafen, user, reason);
		}

		private void FollowupActions(PunishmentType punishmentType, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Removal[punishmentType]} {user.FormatUser()}. ");
			if (IsValid && Timers.RemovePunishments(user.Id, punishmentType) > 0)
			{
				sb.Append($"Removed all timed {punishmentType.EnumName().FormatTitle().ToLower()} punishments on them. ");
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
