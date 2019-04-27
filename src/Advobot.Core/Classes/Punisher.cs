using System;
using System.Collections.Generic;
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
		private static Dictionary<Punishment, string> Given { get; } = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "kicked" },
			{ Punishment.Ban, "banned" },
			{ Punishment.Deafen, "deafened" },
			{ Punishment.VoiceMute, "voice-muted" },
			{ Punishment.RoleMute, "role-muted" },
			{ Punishment.Softban, "softbanned" }
		};
		/// <summary>
		/// Strings for saying the type of punishment removed.
		/// </summary>
		private static Dictionary<Punishment, string> Removed { get; } = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "unkicked" }, //Doesn't make sense
			{ Punishment.Ban, "unbanned" },
			{ Punishment.Deafen, "undeafened" },
			{ Punishment.VoiceMute, "unvoice-muted" },
			{ Punishment.RoleMute, "unrole-muted" },
			{ Punishment.Softban, "unsoftbanned" } //Doesn't make sense either
		};

		private readonly List<string> _Actions = new List<string>();
		private readonly TimeSpan? _Time;
		private readonly ITimerService? _Timers;

		/// <summary>
		/// Creates an instance of <see cref="Punisher"/>.
		/// </summary>
		/// <param name="time">How long to give a punishment for. Removing punishments is not affected by this.</param>
		/// <param name="timers">The timer service to add timed punishments to.</param>
		public Punisher(TimeSpan? time, ITimerService? timers)
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
		public async Task BanAsync(SocketGuild guild, ulong userId, RequestOptions? options = null, int days = 1)
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
		public async Task SoftbanAsync(SocketGuild guild, ulong userId, RequestOptions? options = null)
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
		public async Task KickAsync(SocketGuildUser user, RequestOptions? options = null)
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
		public async Task RoleMuteAsync(SocketGuildUser user, IRole role, RequestOptions? options = null)
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
		public async Task VoiceMuteAsync(SocketGuildUser user, RequestOptions? options = null)
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
		public async Task DeafenAsync(SocketGuildUser user, RequestOptions? options = null)
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
		public Task GiveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, RequestOptions? options = null) => type switch
		{
			Punishment.Ban => BanAsync(guild, userId, options),
			Punishment.Softban => SoftbanAsync(guild, userId, options),
			Punishment.Kick => guild.GetUser(userId) is SocketGuildUser user ? KickAsync(user, options) : Task.CompletedTask,
			Punishment.Deafen => guild.GetUser(userId) is SocketGuildUser user ? DeafenAsync(user, options) : Task.CompletedTask,
			Punishment.VoiceMute => guild.GetUser(userId) is SocketGuildUser user ? VoiceMuteAsync(user, options) : Task.CompletedTask,
			Punishment.RoleMute => guild.GetUser(userId) is SocketGuildUser user && guild.GetRole(roleId) is SocketRole role ? RoleMuteAsync(user, role, options) : Task.CompletedTask,
			_ => throw new InvalidOperationException(nameof(type)),
		};
		private async Task AfterGiveAsync(Punishment type, SocketGuild guild, IUser user, RequestOptions? options = null)
		{
			var sb = new StringBuilder($"Successfully {Given[type]} `{user.Format()}`. ");
			if (_Time != null && _Timers != null)
			{
				//Removing the punishments via the timers in whatever time is set
				sb.Append($"They will be {Removed[type]} in `{_Time}` minutes. ");
				await _Timers.AddAsync(new RemovablePunishment(_Time.Value, type, guild, user)).CAF();
			}
			if (options?.AuditLogReason != null)
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
		public async Task UnbanAsync(SocketGuild guild, ulong userId, RequestOptions? options = null)
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
		public async Task UnRoleMuteAsync(SocketGuildUser user, IRole role, RequestOptions? options = null)
		{
			await user.RemoveRoleAsync(role, options).CAF();
			await AfterRemoveAsync(Punishment.RoleMute, user.Guild, user, options).CAF();
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UnvoicemuteAsync(SocketGuildUser user, RequestOptions? options = null)
		{
			await user.ModifyAsync(x => x.Mute = false, options).CAF();
			await AfterRemoveAsync(Punishment.VoiceMute, user.Guild, user, options).CAF();
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task UndeafenAsync(SocketGuildUser user, RequestOptions? options = null)
		{
			await user.ModifyAsync(x => x.Deaf = false, options).CAF();
			await AfterRemoveAsync(Punishment.Deafen, user.Guild, user, options).CAF();
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
		public Task RemoveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, RequestOptions? options = null) => type switch
		{
			Punishment.Ban => UnbanAsync(guild, userId, options),
			Punishment.Softban => Task.CompletedTask,
			Punishment.Kick => Task.CompletedTask,
			Punishment.Deafen => guild.GetUser(userId) is SocketGuildUser user ? UndeafenAsync(user, options) : Task.CompletedTask,
			Punishment.VoiceMute => guild.GetUser(userId) is SocketGuildUser user ? UnvoicemuteAsync(user, options) : Task.CompletedTask,
			Punishment.RoleMute => guild.GetUser(userId) is SocketGuildUser user && guild.GetRole(roleId) is SocketRole role ? UnRoleMuteAsync(user, role, options) : Task.CompletedTask,
			_ => throw new InvalidOperationException(nameof(type)),
		};
		private async Task AfterRemoveAsync(Punishment type, SocketGuild guild, IUser user, RequestOptions? options = null)
		{
			var sb = new StringBuilder($"Successfully {Removed[type]} `{user?.Format() ?? "`Unknown User`"}`. ");
			if (_Timers != null && (await _Timers.RemovePunishmentAsync(guild.Id, user?.Id ?? 0, type).CAF()).UserId != 0)
			{
				sb.Append($"Removed all timed {type.ToString().FormatTitle().ToLower()} punishments on them. ");
			}
			if (options?.AuditLogReason != null)
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
			=> string.Join("\n", _Actions);
	}
}
