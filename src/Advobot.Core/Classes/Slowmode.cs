using Advobot.Core.Actions.Formatting;
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
			this.BaseMessages = baseMessages;
			this.Interval = interval;
			this.ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
		}

		public void Disable() => this.Enabled = false;
		public void Enable() => this.Enabled = true;

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"**Base messages:** `{this.BaseMessages}`")
			.AppendLineFeed($"**Time interval:** `{this.Interval}`")
			.Append($"**Immune Role Ids:** `{String.Join("`, `", this.ImmuneRoleIds)}`").ToString();
		public string ToString(SocketGuild guild)
			=> new StringBuilder()
			.AppendLineFeed($"**Base messages:** `{this.BaseMessages}`")
			.AppendLineFeed($"**Time interval:** `{this.Interval}`")
			.Append($"**Immune Roles:** `{String.Join("`, `", this.ImmuneRoleIds.Select(x => guild.GetRole(x).FormatRole()))}`").ToString();
	}
}
