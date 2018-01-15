using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
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
		public bool Enabled { get; private set; } = false;

		public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
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
			return new StringBuilder()
					   .AppendLineFeed($"**Base messages:** `{BaseMessages}`")
					   .AppendLineFeed($"**Time interval:** `{Interval}`")
					   .Append($"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds)}`").ToString();
		}

		public string ToString(SocketGuild guild)
		{
			return new StringBuilder()
					   .AppendLineFeed($"**Base messages:** `{BaseMessages}`")
					   .AppendLineFeed($"**Time interval:** `{Interval}`")
					   .Append($"**Immune Roles:** `{String.Join("`, `", ImmuneRoleIds.Select(x => guild.GetRole(x).FormatRole()))}`").ToString();
		}
	}
}
