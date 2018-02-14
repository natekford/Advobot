using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Limits the amount of messages users are allowed to send in a given time interval. Initially created with <see cref="Enabled"/> set to false.
	/// </summary>
	public class Slowmode : IGuildSetting
	{
		[JsonProperty]
		public int BaseMessages { get; }
		[JsonProperty("Interval")]
		private int _Interval;
		[JsonProperty("ImmuneRoleIds")]
		private ulong[] _ImmuneRoleIds;
		[JsonIgnore]
		public TimeSpan Interval => TimeSpan.FromSeconds(_Interval);
		[JsonIgnore]
		public ImmutableList<ulong> ImmuneRoleIds => _ImmuneRoleIds.ToImmutableList();
		[JsonIgnore]
		public bool Enabled;

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			_Interval = interval;
			_ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{

		}

		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{_Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", _ImmuneRoleIds)}`";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{_Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds.Select(x => guild.GetRole(x).Format()))}`";
		}
	}
}
