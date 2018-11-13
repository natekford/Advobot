using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Punishments that will be removed after the time has passed.
	/// </summary>
	public class RemovablePunishment : DatabaseEntry
	{
		private static RequestOptions PunishmentReason { get; } = DiscordUtils.GenerateRequestOptions("Automatic punishment removal.");

		/// <summary>
		/// The type of punishment that was given.
		/// </summary>
		public Punishment PunishmentType { get; set; }
		/// <summary>
		/// The id of the guild the punishment was given on.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the user the punishment was given to.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The id of the role given (only applicable if <see cref="PunishmentType"/> is <see cref="Punishment.RoleMute"/>).
		/// </summary>
		public ulong RoleId { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>. Parameterless constructor is used for the database.
		/// </summary>
		public RemovablePunishment() : base(default) { }
		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="punishment"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user) : base(time)
		{
			PunishmentType = punishment;
			GuildId = guild.Id;
			UserId = user.Id;
			RoleId = 0;
		}
		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <param name="role"></param>
		public RemovablePunishment(TimeSpan time, IRole role, IGuild guild, IUser user) : this(time, Punishment.RoleMute, guild, user)
		{
			RoleId = role.Id;
		}

		/// <summary>
		/// Processes the removable punishments in a way which is more efficient.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="punisher"></param>
		/// <param name="punishments"></param>
		/// <returns></returns>
		public static async Task ProcessRemovablePunishments(
			BaseSocketClient client,
			Punisher punisher,
			IEnumerable<RemovablePunishment> punishments)
		{
			foreach (var guildGroup in punishments.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(guildGroup.Key) is SocketGuild guild))
				{
					continue;
				}
				foreach (var punishmentGroup in guildGroup.GroupBy(x => x.PunishmentType))
				{
					await Task.WhenAll(punishmentGroup.Select(x => Handle(guild, punisher, x))).CAF();
				}
			}
		}
		/// <summary>
		/// Simple case for the punishment type.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="punisher"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		private static async Task Handle(SocketGuild guild, Punisher punisher, RemovablePunishment p)
		{
			switch (p.PunishmentType)
			{
				case Punishment.Ban:
					await punisher.UnbanAsync(guild, p.UserId, PunishmentReason).CAF();
					return;
				case Punishment.Deafen:
					await punisher.UndeafenAsync(guild.GetUser(p.UserId), PunishmentReason).CAF();
					return;
				case Punishment.VoiceMute:
					await punisher.UnvoicemuteAsync(guild.GetUser(p.UserId), PunishmentReason).CAF();
					return;
				case Punishment.RoleMute:
					await punisher.UnrolemuteAsync(guild.GetUser(p.UserId), guild.GetRole(p.RoleId), PunishmentReason).CAF();
					return;
			}
		}
	}
}
