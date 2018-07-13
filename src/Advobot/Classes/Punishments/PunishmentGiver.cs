using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	/// <summary>
	/// Gives a punishment to a user.
	/// </summary>
	/// <inheritdoc cref="PunishmentBase"/>
	public class PunishmentGiver : PunishmentBase
	{
		private TimeSpan _Time;

		/// <summary>
		/// Creates an instance of punishment giver with the time for each punishment being the int passed in in minutes.
		/// </summary>
		/// <param name="minutes"></param>
		/// <param name="timers"></param>
		public PunishmentGiver(int minutes, ITimersService timers) : base(timers)
		{
			_Time = TimeSpan.FromMinutes(minutes);
		}
		/// <summary>
		/// Creates an instance of punishment giver with the time for each punishment being the passed in timespan.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="timers"></param>
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
			await After(Punishment.Ban, guild, ban?.User, options).CAF();
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
			await After(Punishment.Softban, guild, ban?.User, options).CAF();
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
			await After(Punishment.Kick, user?.Guild, user, options).CAF();
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
			await After(Punishment.RoleMute, user?.Guild, user, options).CAF();
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
			await After(Punishment.VoiceMute, user?.Guild, user, options).CAF();
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
			await After(Punishment.Deafen, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Does an action on a user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task PunishAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, RequestOptions options)
		{
			//Ban and softban both work without having to get the user
			switch (type)
			{
				case Punishment.Ban:
					await BanAsync(guild, userId, options).CAF();
					return;
				case Punishment.Softban:
					await SoftbanAsync(guild, userId, options).CAF();
					return;
			}

			if (!(guild.GetUser(userId) is SocketGuildUser user))
			{
				return;
			}
			switch (type)
			{
				case Punishment.Kick:
					await KickAsync(user, options).CAF();
					return;
				case Punishment.Deafen:
					await DeafenAsync(user, options).CAF();
					return;
				case Punishment.VoiceMute:
					await VoiceMuteAsync(user, options).CAF();
					return;
				case Punishment.RoleMute:
					if (!(guild.GetRole(roleId) is SocketRole role))
					{
						return;
					}
					await RoleMuteAsync(user, role, options).CAF();
					return;
			}
		}

		private async Task After(Punishment type, IGuild guild, IUser user, RequestOptions options)
		{
			var sb = new StringBuilder($"Successfully {Given[type]} `{user.Format()}`. ");
			if (!_Time.Equals(default) && Timers != null)
			{
				//Removing the punishments via the timers in whatever time is set
				await Timers.AddAsync(new RemovablePunishment(_Time, type, guild, user)).CAF();
				sb.Append($"They will be {Removed[type]} in `{_Time}` minutes. ");
			}
			if (options.AuditLogReason != null)
			{
				sb.Append($"The provided reason is `{options.AuditLogReason.EscapeBackTicks().TrimEnd('.', ' ')}`. ");
			}
			Actions.Add(sb.ToString().Trim());
		}
	}
}
