using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities for punishing users and removing punishments.
	/// </summary>
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
				await AfterGiveAsync(Punishment.Softban, guild, ban.User, options).CAF();
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
				await AfterGiveAsync(Punishment.Softban, guild, ban.User, options).CAF();
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
			await AfterGiveAsync(Punishment.Kick, user.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.RoleMute, user.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.VoiceMute, user.Guild, user, options).CAF();
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
			await AfterGiveAsync(Punishment.Deafen, user.Guild, user, options).CAF();
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
		public static Task GiveAsync(Punishment type, IGuild guild, ulong userId, ulong? roleId, PunishmentArgs? options = null) => type switch
		{
			Punishment.Ban => BanAsync(guild, userId, options: options),
			Punishment.Softban => SoftbanAsync(guild, userId, options),
			Punishment.Kick => RequireUser(KickAsync, guild, userId, options),
			Punishment.Deafen => RequireUser(DeafenAsync, guild, userId, options),
			Punishment.VoiceMute => RequireUser(VoiceMuteAsync, guild, userId, options),
			Punishment.RoleMute => RequireUserAndRole(RoleMuteAsync, guild, userId, roleId, options),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
		private static Task AfterGiveAsync(Punishment type, IGuild guild, IUser user, PunishmentArgs options)
		{
			if (options.Timers != null && options.Time != null)
			{
				options.Timers.Add(new RemovablePunishment(options.Time.Value, type, guild, user));
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
		public static Task RemoveAsync(Punishment type, IGuild guild, ulong userId, ulong? roleId, PunishmentArgs? options = null) => type switch
		{
			Punishment.Ban => UnbanAsync(guild, userId, options),
			Punishment.Softban => Task.CompletedTask,
			Punishment.Kick => Task.CompletedTask,
			Punishment.Deafen => RequireUser(RemoveDeafenAsync, guild, userId, options),
			Punishment.VoiceMute => RequireUser(RemoveVoiceMuteAsync, guild, userId, options),
			Punishment.RoleMute => RequireUserAndRole(RemoveRoleMuteAsync, guild, userId, roleId, options),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
		private static Task AfterRemoveAsync(Punishment type, IGuild guild, IUser user, PunishmentArgs options)
		{
			if (options.Timers != null)
			{
				if (options.Timers.RemovePunishmentAsync(guild.Id, user.Id, type))
				{
					((PunishmentArgs.IPunishmentRemoved)options).SetPunishmentRemoved();
				}
			}
			return Task.CompletedTask;
		}

		private static async Task RequireUser(Func<IGuildUser, PunishmentArgs?, Task> f, IGuild guild, ulong userId, PunishmentArgs? options)
		{
			var user = await guild.GetUserAsync(userId, options: options?.Options).CAF();
			if (user != null)
			{
				await f(user, options).CAF();
			}
		}
		private static async Task RequireUserAndRole(Func<IGuildUser, IRole, PunishmentArgs?, Task> f, IGuild guild, ulong userId, ulong? roleId, PunishmentArgs? options)
		{
			if (roleId == null)
			{
				return;
			}

			var user = await guild.GetUserAsync(userId, options: options?.Options).CAF();
			var role = guild.GetRole(roleId.Value);
			if (user != null && role != null)
			{
				await f(user, role, options).CAF();
			}
		}
	}
}
