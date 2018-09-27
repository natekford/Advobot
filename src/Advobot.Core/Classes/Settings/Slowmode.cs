using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Limits the amount of messages users are allowed to send in a given time interval. Initially created with <see cref="Enabled"/> set to false.
	/// </summary>
	public class Slowmode : IGuildSetting
	{
		/// <summary>
		/// The base messages every user gets to start with.
		/// </summary>
		[JsonProperty]
		public int BaseMessages { get; private set; }
		/// <summary>
		/// How long until messages refresh for the user in seconds.
		/// </summary>
		[JsonProperty]
		public int TimeInterval { get; private set; }
		/// <summary>
		/// Roles that are immune from slowmode.
		/// </summary>
		[JsonProperty]
		public IList<ulong> ImmuneRoleIds { get; } = new List<ulong>();
		/// <summary>
		/// Whether or not slowmode is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; set; }
		/// <summary>
		/// <see cref="TimeInterval"/> as a <see cref="TimeSpan"/>.
		/// </summary>
		[JsonIgnore]
		public TimeSpan IntervalTimeSpan => TimeSpan.FromSeconds(TimeInterval);

		/// <summary>
		/// Creates an instance of <see cref="Slowmode"/>.
		/// </summary>
		public Slowmode() { }
		/// <summary>
		/// Creates an instance of <see cref="Slowmode"/>.
		/// </summary>
		/// <param name="baseMessages"></param>
		/// <param name="interval"></param>
		/// <param name="immuneRoles"></param>
		public Slowmode(int baseMessages, int interval, IEnumerable<IRole> immuneRoles)
		{
			BaseMessages = baseMessages;
			TimeInterval = interval;
			ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToList();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{TimeInterval}`\n" +
				$"**Immune Role Ids:** `{string.Join("`, `", ImmuneRoleIds)}`";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{TimeInterval}`\n" +
				$"**Immune Role Ids:** `{ImmuneRoleIds.Join("`, `", x => guild.GetRole(x).Format())}`";
		}
	}
}
