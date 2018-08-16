using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;

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
		public int BaseMessages { get; }
		/// <summary>
		/// How long until messages refresh for the user.
		/// </summary>
		[JsonIgnore]
		public TimeSpan Interval => TimeSpan.FromSeconds(_Interval);
		/// <summary>
		/// Roles that are immune from slowmode.
		/// </summary>
		[JsonIgnore]
		public ImmutableList<ulong> ImmuneRoleIds => _ImmuneRoleIds.ToImmutableList();
		/// <summary>
		/// Whether or not slowmode is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; set; }

		[JsonProperty("Interval")]
		private int _Interval;
		[JsonProperty("ImmuneRoleIds")]
		private ulong[] _ImmuneRoleIds;

		/// <summary>
		/// Creates an instance of slowmode.
		/// </summary>
		/// <param name="baseMessages"></param>
		/// <param name="interval"></param>
		/// <param name="immuneRoles"></param>
		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			_Interval = interval;
			_ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{_Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", _ImmuneRoleIds)}`";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{_Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds.Select(x => guild.GetRole(x).Format()))}`";
		}
	}
}
