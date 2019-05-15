using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	public static class PunishmentUtils
	{
		/// <summary>
		/// Bans a user from the guild. <paramref name="days"/> specifies how many days worth of messages to delete.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <param name="days"></param>
		/// <returns></returns>
		public static async Task BanAsync(IGuild guild, ulong userId, int days = 1, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await guild.AddBanAsync(userId, days, null, options.Options).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			if (ban != null && ban.User != null)
			{
				await AfterGiveAsync(Punishment.Softban, guild, ban.User, options);
			}
		}
		/// <summary>
		/// Bans then unbans a user from the guild. Deletes 1 days worth of messages.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task SoftbanAsync(IGuild guild, ulong userId, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await guild.AddBanAsync(userId, 1, null, options.Options).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, options.Options).CAF();
			if (ban != null && ban.User != null)
			{
				await AfterGiveAsync(Punishment.Softban, guild, ban.User, options);
			}
		}
		/// <summary>
		/// Kicks a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task KickAsync(IGuildUser user, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.KickAsync(null, options.Options).CAF();
			await AfterGiveAsync(Punishment.Kick, user.Guild, user, options);
		}
		/// <summary>
		/// Gives a user the mute role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task RoleMuteAsync(IGuildUser user, IRole role, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.AddRoleAsync(role, options.Options).CAF();
			await AfterGiveAsync(Punishment.RoleMute, user.Guild, user, options);
		}
		/// <summary>
		/// Mutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task VoiceMuteAsync(IGuildUser user, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.ModifyAsync(x => x.Mute = true, options.Options).CAF();
			await AfterGiveAsync(Punishment.VoiceMute, user.Guild, user, options);
		}
		/// <summary>
		/// Deafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task DeafenAsync(IGuildUser user, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.ModifyAsync(x => x.Deaf = true, options.Options).CAF();
			await AfterGiveAsync(Punishment.Deafen, user.Guild, user, options);
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
		public static Task GiveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, PunishmentArgs? options = null) => type switch
		{
			Punishment.Ban => BanAsync(guild, userId, options: options),
			Punishment.Softban => SoftbanAsync(guild, userId, options),
			Punishment.Kick => guild.GetUser(userId) is IGuildUser user ? KickAsync(user, options) : Task.CompletedTask,
			Punishment.Deafen => guild.GetUser(userId) is IGuildUser user ? DeafenAsync(user, options) : Task.CompletedTask,
			Punishment.VoiceMute => guild.GetUser(userId) is IGuildUser user ? VoiceMuteAsync(user, options) : Task.CompletedTask,
			Punishment.RoleMute => guild.GetUser(userId) is IGuildUser user && guild.GetRole(roleId) is IRole role ? RoleMuteAsync(user, role, options) : Task.CompletedTask,
			_ => throw new ArgumentException(nameof(type)),
		};
		private static Task AfterGiveAsync(Punishment type, IGuild guild, IUser user, PunishmentArgs options)
		{
			if (options.Timers != null && options.Time != null)
			{
				return options.Timers.AddAsync(new RemovablePunishment(options.Time.Value, type, guild, user));
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// Removes a user from the ban list.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task UnbanAsync(IGuild guild, ulong userId, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			var ban = (await guild.GetBansAsync().CAF()).SingleOrDefault(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, options.Options).CAF();
			if (ban != null && ban.User != null)
			{
				await AfterRemoveAsync(Punishment.Ban, guild, ban.User, options).CAF();
			}
		}
		/// <summary>
		/// Removes the mute role from the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task RemoveRoleMuteAsync(IGuildUser user, IRole role, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.RemoveRoleAsync(role, options.Options).CAF();
			await AfterRemoveAsync(Punishment.RoleMute, user.Guild, user, options).CAF();
		}
		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task RemoveVoiceMuteAsync(IGuildUser user, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.ModifyAsync(x => x.Mute = false, options.Options).CAF();
			await AfterRemoveAsync(Punishment.VoiceMute, user.Guild, user, options).CAF();
		}
		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task RemoveDeafenAsync(IGuildUser user, PunishmentArgs? options = null)
		{
			options ??= PunishmentArgs.Default;
			await user.ModifyAsync(x => x.Deaf = false, options.Options).CAF();
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
		public static Task RemoveAsync(Punishment type, SocketGuild guild, ulong userId, ulong roleId, PunishmentArgs? options = null) => type switch
		{
			Punishment.Ban => UnbanAsync(guild, userId, options),
			Punishment.Softban => Task.CompletedTask,
			Punishment.Kick => Task.CompletedTask,
			Punishment.Deafen => guild.GetUser(userId) is IGuildUser user ? RemoveDeafenAsync(user, options) : Task.CompletedTask,
			Punishment.VoiceMute => guild.GetUser(userId) is IGuildUser user ? RemoveVoiceMuteAsync(user, options) : Task.CompletedTask,
			Punishment.RoleMute => guild.GetUser(userId) is IGuildUser user && guild.GetRole(roleId) is IRole role ? RemoveRoleMuteAsync(user, role, options) : Task.CompletedTask,
			_ => throw new ArgumentException(nameof(type)),
		};
		private static async Task AfterRemoveAsync(Punishment type, IGuild guild, IUser user, PunishmentArgs options)
		{
			if (options.Timers != null)
			{
				var punishment = await options.Timers.RemovePunishmentAsync(guild.Id, user.Id, type).CAF();
				if (punishment != null)
				{
					((IPunishmentRemoved)options).SetPunishmentRemoved();
				}
			}
		}
	}

	internal interface IPunishmentRemoved
	{
		void SetPunishmentRemoved();
	}

	public sealed class PunishmentArgs : IPunishmentRemoved
	{
		public static readonly PunishmentArgs Default = new PunishmentArgs();

		public TimeSpan? Time { get; set; }
		public ITimerService? Timers { get; set; }
		public RequestOptions? Options { get; set; }
		public bool PunishmentRemoved { get; private set; }

		void IPunishmentRemoved.SetPunishmentRemoved()
			=> PunishmentRemoved = true;
	}
}
