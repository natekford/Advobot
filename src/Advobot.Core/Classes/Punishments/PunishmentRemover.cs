using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
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
		public PunishmentRemover(ITimersService timers) : base(timers) { }

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
			After(PunishmentType.Ban, guild, ban?.User, reason);
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
			await RoleUtils.TakeRolesAsync(user, new[] { role }, reason).CAF();
			After(PunishmentType.RoleMute, user?.Guild, user, reason);
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UnvoicemuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user?.ModifyAsync(x => x.Mute = false, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.VoiceMute, user?.Guild, user, reason);
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task UndeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user?.ModifyAsync(x => x.Deaf = false, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.Deafen, user?.Guild, user, reason);
		}

		protected override void After(PunishmentType type, IGuild guild, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Removal[type]} {user?.Format() ?? "`Unknown User`"}. ");
			if (_Timers != null && _Timers.RemovePunishments(user?.Id ?? 0, type) > 0)
			{
				sb.Append($"Removed all timed {type.EnumName().FormatTitle().ToLower()} punishments on them. ");
			}
			if (reason.Reason != null)
			{
				sb.Append($"The provided reason is `{reason.Reason.EscapeBackTicks()}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}
	}
}
