using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Limits the amount of messages users are allowed to send in a given time interval. Initially created with <see cref="Enabled"/> set to false.
	/// </summary>
	public class Slowmode : ISetting
	{
		[JsonProperty]
		public readonly int BaseMessages;
		[JsonProperty]
		public readonly int Interval;
		[JsonProperty]
		public readonly ulong[] ImmuneRoleIds;
		[JsonIgnore]
		public bool Enabled { get; private set; }

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
			Enabled = false;
		}

		public void Disable()
		{
			Enabled = false;
		}
		public void Enable()
		{
			Enabled = true;
		}

		public override string ToString()
		{
			return $"**Base messages:** `{BaseMessages}`\n" +
					$"**Time interval:** `{Interval}`\n" +
					$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds)}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
