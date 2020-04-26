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
			await AfterGiveAsync(PunishmentType.Ban, user.Id, args).CAF();
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
			await AfterGiveAsync(PunishmentType.Deafen, retrieved, args).CAF();
		}

		/// <summary>
		/// Gives the specified punishment type to the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public Task GiveAsync(
			PunishmentType type,
			AmbiguousUser user,
			PunishmentArgs? args = null
		) => type switch
		{
			PunishmentType.Ban => BanAsync(user, args: args),
			PunishmentType.Softban => SoftbanAsync(user, args),
			PunishmentType.Kick => KickAsync(user, args),
			PunishmentType.Deafen => DeafenAsync(user, args),
			PunishmentType.VoiceMute => VoiceMuteAsync(user, args),
			PunishmentType.RoleMute => RoleMuteAsync(user, args),
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
			await AfterGiveAsync(PunishmentType.Kick, retrieved, args).CAF();
		}

		/// <summary>
		/// Removes the specified punishment type from the user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public Task RemoveAsync(
			PunishmentType type,
			AmbiguousUser user,
			PunishmentArgs? args = null
		) => type switch
		{
			PunishmentType.Ban => UnbanAsync(user, args),
			PunishmentType.Softban => Task.CompletedTask,
			PunishmentType.Kick => Task.CompletedTask,
			PunishmentType.Deafen => RemoveDeafenAsync(user, args),
			PunishmentType.VoiceMute => RemoveVoiceMuteAsync(user, args),
			PunishmentType.RoleMute => RemoveRoleMuteAsync(user, args),
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
			await AfterRemoveAsync(PunishmentType.Deafen, retrieved, args).CAF();
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
			await AfterRemoveAsync(PunishmentType.RoleMute, retrieved, args).CAF();
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
			await AfterRemoveAsync(PunishmentType.VoiceMute, retrieved, args).CAF();
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
			await AfterGiveAsync(PunishmentType.RoleMute, retrieved, args).CAF();
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
			await AfterGiveAsync(PunishmentType.Softban, user.Id, args).CAF();
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
			await AfterRemoveAsync(PunishmentType.Ban, user.Id, args).CAF();
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
			await AfterGiveAsync(PunishmentType.VoiceMute, retrieved, args).CAF();
		}

		private Task AfterGiveAsync(
			PunishmentType type,
			IUser user,
			PunishmentArgs? args
		) => AfterGiveAsync(type, user.Id, args);

		private Task AfterGiveAsync(
			PunishmentType type,
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
			PunishmentType type,
			IUser user,
			PunishmentArgs? args
		) => AfterRemoveAsync(type, user.Id, args);

		private Task AfterRemoveAsync(
			PunishmentType type,
			ulong userId,
			PunishmentArgs? args)
		{
			if (Timers?.RemovePunishment(Guild.Id, userId, type) == true && args != null)
			{
				args.PunishmentRemoved = true;
			}
			return Task.CompletedTask;
		}
	}
}