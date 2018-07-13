using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	/// <summary>
	/// Removes a punishment from a user.
	/// </summary>
	public class PunishmentRemover : PunishmentBase
	{
		/// <summary>
		/// Creates an instance of punishment remover.
		/// </summary>
		/// <param name="timers"></param>
		public PunishmentRemover(ITimersService timers) : base(timers) { }

		/// <summary>
		/// Removes a user from the ban list.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnbanAsync(IGuild guild, ulong userId, RequestOptions options)
		{
			var ban = (await guild.GetBansAsync().CAF()).SingleOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, options).CAF();
			await After(Punishment.Ban, guild, ban?.User, options).CAF();
		}
		/// <summary>
		/// Removes the mute role from the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnrolemuteAsync(IGuildUser user, IRole role, RequestOptions options)
		{
			await user.RemoveRoleAsync(role, options).CAF();
			await After(Punishment.RoleMute, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnvoicemuteAsync(IGuildUser user, RequestOptions options)
		{
			if (user != null)
			{
				await user.ModifyAsync(x => x.Mute = false, options).CAF();
				await After(Punishment.VoiceMute, user.Guild, user, options).CAF();
			}
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UndeafenAsync(IGuildUser user, RequestOptions options)
		{
			if (user != null)
			{
				await user.ModifyAsync(x => x.Deaf = false, options).CAF();
				await After(Punishment.Deafen, user.Guild, user, options).CAF();
			}
		}

		private async Task After(Punishment type, IGuild guild, IUser user, RequestOptions options)
		{
			var sb = new StringBuilder($"Successfully {Removed[type]} `{user?.Format() ?? "`Unknown User`"}`. ");
			if (Timers != null && (await Timers.RemovePunishmentAsync(guild, user?.Id ?? 0, type).CAF()).UserId != 0)
			{
				sb.Append($"Removed all timed {type.ToString().FormatTitle().ToLower()} punishments on them. ");
			}
			if (options.AuditLogReason != null)
			{
				sb.Append($"The provided reason is `{options.AuditLogReason.EscapeBackTicks().TrimEnd('.', ' ')}`. ");
			}
			Actions.Add(sb.ToString().Trim());
		}
	}
}
