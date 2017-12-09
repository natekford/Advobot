using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Groups <see cref="SelfAssignableRole"/> together.
	/// </summary>
	public class SelfAssignableGroup : ISetting
	{
		[JsonProperty]
		public List<SelfAssignableRole> Roles { get; private set; } = new List<SelfAssignableRole>();
		[JsonProperty]
		public int Group { get; private set; }

		public SelfAssignableGroup(int group)
		{
			this.Group = group;
		}

		public void AddRoles(IEnumerable<SelfAssignableRole> roles) => this.Roles.AddRange(roles);
		public void RemoveRoles(IEnumerable<ulong> roleIDs) => this.Roles.RemoveAll(x => roleIDs.Contains(x.RoleId));

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"`Group: {this.Group}`")
			.Append(String.Join("\n", this.Roles.Select(x => x.ToString()))).ToString();
		public string ToString(SocketGuild guild) => ToString();
	}
}