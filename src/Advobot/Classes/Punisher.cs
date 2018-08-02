using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles giving and removing certain punishments on a user.
	/// </summary>
	public sealed class Punisher
	{
		/// <summary>
		/// Strings for saying the type of punishment given.
		/// </summary>
		private readonly static ImmutableDictionary<Punishment, string> Given = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "kicked" },
			{ Punishment.Ban, "banned" },
			{ Punishment.Deafen, "deafened" },
			{ Punishment.VoiceMute, "voice-muted" },
			{ Punishment.RoleMute, "role-muted" },
			{ Punishment.Softban, "softbanned" }
		}.ToImmutableDictionary();
		/// <summary>
		/// Strings for saying the type of punishment removed.
		/// </summary>
		private readonly static ImmutableDictionary<Punishment, string> Removed = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "unkicked" }, //Doesn't make sense
			{ Punishment.Ban, "unbanned" },
			{ Punishment.Deafen, "undeafened" },
			{ Punishment.VoiceMute, "unvoice-muted" },
			{ Punishment.RoleMute, "unrole-muted" },
			{ Punishment.Softban, "unsoftbanned" } //Doesn't make sense either
		}.ToImmutableDictionary();

		/// <summary>
		/// The actions which were done on users.
		/// </summary>
		private readonly List<string> _Actions = new List<string>();
		/// <summary>
		/// How long to give punishments for.
		/// </summary>
		private readonly TimeSpan _Time;
		/// <summary>
		/// The timer service to add punishments to or remove punishments from.
		/// </summary>
		private readonly ITimersService _Timers;

		/// <summary>
		/// Creates an instance of <see cref="Punisher"/>.
		/// </summary>
		/// <param name="time">How long to give a punishment for. Removing punishments is not affected by this.</param>
		/// <param name="timers">The timer service to add timed punishments to.</param>
		public Punisher(TimeSpan time, ITimersService timers)
		{
			_Time = time;
			_Timers = timers;
		}

		#region Give
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
			await AfterGiveAsync(Punishment.Ban, guild, ban?.User, options).CAF();
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
			await AfterGiveAsync(Punishment.Softban, guild, ban?.User, options).CAF();
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
			await AfterGiveAsync(Punishment.Kick, user?.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.RoleMute, user?.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.VoiceMute, user?.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.Deafen, user?.Guild, user, options).CAF();
		}
		/// <summary>
		/// Gives the specified punishment type to the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task GiveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, RequestOptions options)
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
		private async Task AfterGiveAsync(Punishment type, SocketGuild guild, IUser user, RequestOptions options)
		{
			var sb = new StringBuilder($"Successfully {Given[type]} `{user.Format()}`. ");
			if (!_Time.Equals(default) && _Timers != null)
			{
				//Removing the punishments via the timers in whatever time is set
				await _Timers.AddAsync(new RemovablePunishment(_Time, type, guild, user)).CAF();
				sb.Append($"They will be {Removed[type]} in `{_Time}` minutes. ");
			}
			if (options.AuditLogReason != null)
			{
				sb.Append($"The provided reason is `{options.AuditLogReason.EscapeBackTicks().TrimEnd('.', ' ')}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}
		#endregion

		#region Remove
		/// <summary>
		/// Removes a user from the ban list.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnbanAsync(SocketGuild guild, ulong userId, RequestOptions options)
		{
			var ban = (await guild.GetBansAsync().CAF()).SingleOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, options).CAF();
			await AfterRemoveAsync(Punishment.Ban, guild, ban?.User, options).CAF();
		}
		/// <summary>
		/// Removes the mute role from the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnrolemuteAsync(SocketGuildUser user, IRole role, RequestOptions options)
		{
			if (user != null)
			{
				await user.RemoveRoleAsync(role, options).CAF();
				await AfterRemoveAsync(Punishment.RoleMute, user.Guild, user, options).CAF();
			}
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnvoicemuteAsync(SocketGuildUser user, RequestOptions options)
		{
			if (user != null)
			{
				await user.ModifyAsync(x => x.Mute = false, options).CAF();
				await AfterRemoveAsync(Punishment.VoiceMute, user.Guild, user, options).CAF();
			}
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UndeafenAsync(SocketGuildUser user, RequestOptions options)
		{
			if (user != null)
			{
				await user.ModifyAsync(x => x.Deaf = false, options).CAF();
				await AfterRemoveAsync(Punishment.Deafen, user.Guild, user, options).CAF();
			}
		}
		/// <summary>
		/// Removes the specified punishment type from the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task RemoveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, RequestOptions options)
		{
			switch (type)
			{
				case Punishment.Ban:
					await UnbanAsync(guild, userId, options).CAF();
					return;
				//Can't reverse a softban or kick
				case Punishment.Softban:
				case Punishment.Kick:
					return;
			}

			if (!(guild.GetUser(userId) is SocketGuildUser user))
			{
				return;
			}
			switch (type)
			{
				case Punishment.Deafen:
					await UndeafenAsync(user, options).CAF();
					return;
				case Punishment.VoiceMute:
					await UnvoicemuteAsync(user, options).CAF();
					return;
				case Punishment.RoleMute:
					if (!(guild.GetRole(roleId) is SocketRole role))
					{
						return;
					}
					await UnrolemuteAsync(user, role, options).CAF();
					return;
			}
		}
		private async Task AfterRemoveAsync(Punishment type, SocketGuild guild, IUser user, RequestOptions options)
		{
			var sb = new StringBuilder($"Successfully {Removed[type]} `{user?.Format() ?? "`Unknown User`"}`. ");
			if (_Timers != null && (await _Timers.RemovePunishmentAsync(guild.Id, user?.Id ?? 0, type).CAF()).UserId != 0)
			{
				sb.Append($"Removed all timed {type.ToString().FormatTitle().ToLower()} punishments on them. ");
			}
			if (options.AuditLogReason != null)
			{
				sb.Append($"The provided reason is `{options.AuditLogReason.EscapeBackTicks().TrimEnd('.', ' ')}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}
		#endregion

		/// <summary>
		/// Returns all the actions joined together.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Join("\n", _Actions);
		}
	}
}
