using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Collections.Immutable;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Limits the amount of messages users are allowed to send in a given time interval. Initially created with <see cref="Enabled"/> set to false.
	/// </summary>
	public class Slowmode : ISetting
	{
		[JsonProperty]
		public int BaseMessages { get; }
		[JsonProperty]
		public int Interval { get; }
		[JsonProperty("ImmuneRoleIds")]
		private ulong[] _ImmuneRoleIds;
		[JsonIgnore]
		public ImmutableArray<ulong> ImmuneRoleIds => _ImmuneRoleIds.ToImmutableArray();
		[JsonIgnore]
		public bool Enabled;

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			_ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
		}

		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", _ImmuneRoleIds)}`";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
				$"**Time interval:** `{Interval}`\n" +
				$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds.Select(x => guild.GetRole(x).Format()))}`";
		}
	}
}
