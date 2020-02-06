using System;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Timers;

using AdvorangesUtils;

using Discord;

namespace Advobot.Utilities
{
	/// <summary>
	/// Handles giving and removing punishments.
	/// </summary>
	public sealed class PunishmentManager
	{
		/// <summary>
		/// The guild to add or remove punishments on.
		/// </summary>
		public IGuild Guild { get; }

		/// <summary>
		/// Timers for removing punishments.
		/// </summary>
		public ITimerService? Timers { get; }

		/// <summary>
		/// Creates an instace of <see cref="PunishmentManager"/>.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="timers"></param>
		public PunishmentManager(IGuild guild, ITimerService? timers)
		{
			Guild = guild;
			Timers = timers;
		}

		/// <summary>
		/// Bans a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task BanAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			await Guild.AddBanAsync(user.Id, args?.Days ?? 1, null, args?.Options).CAF();
			await AfterGiveAsync(Punishment.Ban, user.Id, args).CAF();
		}

		/// <summary>
		/// Deafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task DeafenAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			await retrieved.ModifyAsync(x => x.Deaf = true, args?.Options).CAF();
			await AfterGiveAsync(Punishment.Deafen, retrieved, args).CAF();
		}

		/// <summary>
		/// Gives the specified punishment type to the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public Task GiveAsync(
			Punishment type,
			AmbiguousUser user,
			PunishmentArgs? args = null
		) => type switch
		{
			Punishment.Ban => BanAsync(user, args: args),
			Punishment.Softban => SoftbanAsync(user, args),
			Punishment.Kick => KickAsync(user, args),
			Punishment.Deafen => DeafenAsync(user, args),
			Punishment.VoiceMute => VoiceMuteAsync(user, args),
			Punishment.RoleMute => RoleMuteAsync(user, args),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};

		/// <summary>
		/// Kicks a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task KickAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			await retrieved.KickAsync(null, args?.Options).CAF();
			await AfterGiveAsync(Punishment.Kick, retrieved, args).CAF();
		}

		/// <summary>
		/// Removes the specified punishment type from the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public Task RemoveAsync(
			Punishment type,
			AmbiguousUser user,
			PunishmentArgs? args = null
		) => type switch
		{
			Punishment.Ban => UnbanAsync(user, args),
			Punishment.Softban => Task.CompletedTask,
			Punishment.Kick => Task.CompletedTask,
			Punishment.Deafen => RemoveDeafenAsync(user, args),
			Punishment.VoiceMute => RemoveVoiceMuteAsync(user, args),
			Punishment.RoleMute => RemoveRoleMuteAsync(user, args),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};

		/// <summary>
		/// Undeafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task RemoveDeafenAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			await retrieved.ModifyAsync(x => x.Deaf = false, args?.Options).CAF();
			await AfterRemoveAsync(Punishment.Deafen, retrieved, args).CAF();
		}

		/// <summary>
		/// Removes the mute role from the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task RemoveRoleMuteAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			var role = args?.Role;
			await retrieved.RemoveRoleAsync(role, args?.Options).CAF();
			await AfterRemoveAsync(Punishment.RoleMute, retrieved, args).CAF();
		}

		/// <summary>
		/// Unmutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task RemoveVoiceMuteAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			await retrieved.ModifyAsync(x => x.Mute = false, args?.Options).CAF();
			await AfterRemoveAsync(Punishment.VoiceMute, retrieved, args).CAF();
		}

		/// <summary>
		/// Gives a user the mute role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task RoleMuteAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			var role = args?.Role;
			await retrieved.AddRoleAsync(role, args?.Options).CAF();
			await AfterGiveAsync(Punishment.RoleMute, retrieved, args).CAF();
		}

		/// <summary>
		/// Bans then unbans a user from the guild. Deletes 1 days worth of messages.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task SoftbanAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			await Guild.AddBanAsync(user.Id, args?.Days ?? 1, null, args?.Options).CAF();
			await Guild.RemoveBanAsync(user.Id, args?.Options).CAF();
			await AfterGiveAsync(Punishment.Softban, user.Id, args).CAF();
		}

		/// <summary>
		/// Removes a user from the ban list.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task UnbanAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			await Guild.RemoveBanAsync(user.Id, args?.Options).CAF();
			await AfterRemoveAsync(Punishment.Ban, user.Id, args).CAF();
		}

		/// <summary>
		/// Mutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task VoiceMuteAsync(AmbiguousUser user, PunishmentArgs? args = null)
		{
			var retrieved = await user.GetAsync(Guild).CAF();
			await retrieved.ModifyAsync(x => x.Mute = true, args?.Options).CAF();
			await AfterGiveAsync(Punishment.VoiceMute, retrieved, args).CAF();
		}

		private Task AfterGiveAsync(
			Punishment type,
			IUser user,
			PunishmentArgs? args
		) => AfterGiveAsync(type, user.Id, args);

		private Task AfterGiveAsync(
			Punishment type,
			ulong userId,
			PunishmentArgs? args)
		{
			if (Timers != null && args?.Time != null)
			{
				Timers.Add(new RemovablePunishment(args.Time.Value, type, Guild.Id, userId));
			}
			return Task.CompletedTask;
		}

		private Task AfterRemoveAsync(
			Punishment type,
			IUser user,
			PunishmentArgs? args
		) => AfterRemoveAsync(type, user.Id, args);

		private Task AfterRemoveAsync(
			Punishment type,
			ulong userId,
			PunishmentArgs? args)
		{
			if (Timers?.RemovePunishment(Guild.Id, userId, type) == true && args != null)
			{
				((PunishmentArgs.IPunishmentRemoved)args).SetPunishmentRemoved();
			}
			return Task.CompletedTask;
		}
	}
}