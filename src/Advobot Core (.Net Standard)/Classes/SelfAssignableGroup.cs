using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Groups <see cref="SelfAssignableRole"/> together.
	/// </summary>
	public class SelfAssignableGroup : ISetting
	{
		[JsonProperty]
		public List<SelfAssignableRole> Roles { get; private set; }
		[JsonProperty]
		public int Group { get; private set; }

		public SelfAssignableGroup(int group)
		{
			Roles = new List<SelfAssignableRole>();
			Group = group;
		}

		public void AddRoles(IEnumerable<SelfAssignableRole> roles)
		{
			Roles.AddRange(roles);
		}
		public void RemoveRoles(IEnumerable<ulong> roleIDs)
		{
			Roles.RemoveAll(x => roleIDs.Contains(x.RoleId));
		}

		public override string ToString()
		{
			return $"`Group: {Group}`\n{String.Join("\n", Roles.Select(x => x.ToString()))}";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}