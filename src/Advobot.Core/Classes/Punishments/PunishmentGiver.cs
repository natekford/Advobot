using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Gives a punishment to a user.
	/// </summary>
	/// <inheritdoc cref="PunishmentBase"/>
	public class PunishmentGiver : PunishmentBase
	{
		private TimeSpan _Time;

		public PunishmentGiver(int minutes, ITimersService timers) : base(timers)
		{
			_Time = TimeSpan.FromMinutes(minutes);
		}
		public PunishmentGiver(TimeSpan time, ITimersService timers) : base(timers)
		{
			_Time = time;
		}

		/// <summary>
		/// Bans a user from the guild. <paramref name="days"/> specifies how many days worth of messages to delete.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <param name="days"></param>
		/// <returns></returns>
		public async Task BanAsync(SocketGuild guild, ulong userId, RequestOptions options, int days = 1)
		{
			await guild.AddBanAsync(userId, days, null, options).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			await After(PunishmentType.Ban, guild, ban?.User, options).CAF();
		}
		/// <summary>
		/// Bans then unbans a user from the guild. Deletes 1 days worth of messages.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task SoftbanAsync(SocketGuild guild, ulong userId, RequestOptions options)
		{
			await guild.AddBanAsync(userId, 1, null, options).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, options).CAF();
			await After(PunishmentType.Softban, guild, ban?.User, options).CAF();
		}
		/// <summary>
		/// Kicks a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task KickAsync(SocketGuildUser user, RequestOptions options)
		{
			await user.KickAsync(null, options).CAF();
			await After(PunishmentType.Kick, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Gives a user the mute role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task RoleMuteAsync(SocketGuildUser user, IRole role, RequestOptions options)
		{
			await user.AddRoleAsync(role, options).CAF();
			await After(PunishmentType.RoleMute, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Mutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task VoiceMuteAsync(SocketGuildUser user, RequestOptions options)
		{
			await user.ModifyAsync(x => x.Mute = true, options).CAF();
			await After(PunishmentType.VoiceMute, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Deafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task DeafenAsync(SocketGuildUser user, RequestOptions options)
		{
			await user.ModifyAsync(x => x.Deaf = true, options).CAF();
			await After(PunishmentType.Deafen, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Does an action on a user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task PunishAsync(PunishmentType type, SocketGuildUser user, SocketRole role, RequestOptions options)
		{
			if (!(user.Guild is SocketGuild guild && guild.CurrentUser is SocketGuildUser bot && bot.HasHigherPosition(user)))
			{
				return;
			}

			switch (type)
			{
				case PunishmentType.Kick:
					await KickAsync(user, options).CAF();
					return;
				case PunishmentType.Ban:
					await BanAsync(guild, user.Id, options).CAF();
					return;
				case PunishmentType.Deafen:
					await DeafenAsync(user, options).CAF();
					return;
				case PunishmentType.VoiceMute:
					await VoiceMuteAsync(user, options).CAF();
					return;
				case PunishmentType.Softban:
					await SoftbanAsync(guild, user.Id, options).CAF();
					return;
				case PunishmentType.RoleMute:
					await RoleMuteAsync(user, role, options).CAF();
					return;
			}
		}

		private async Task After(PunishmentType type, IGuild guild, IUser user, RequestOptions options)
		{
			var sb = new StringBuilder($"Successfully {_Given[type]} `{user.Format()}`. ");
			if (!_Time.Equals(default) && _Timers != null)
			{
				//Removing the punishments via the timers in whatever time is set
				await _Timers.AddAsync(new RemovablePunishment(_Time, type, guild, user)).CAF();
				sb.Append($"They will be {_Removal[type]} in `{_Time}` minutes. ");
			}
			if (options.AuditLogReason != null)
			{
				sb.Append($"The provided reason is `{options.AuditLogReason.EscapeBackTicks().TrimEnd('.', ' ')}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}
	}
}
