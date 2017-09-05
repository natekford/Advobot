using Advobot.Interfaces;
using Discord;
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

	/// <summary>
	/// Roles which users can assign to themselves via <see cref="Commands.SelfRoles.AssignSelfRole"/>.
	/// </summary>
	public class SelfAssignableRole : ISetting
	{
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public SelfAssignableRole(ulong roleID)
		{
			RoleId = roleID;
		}
		public SelfAssignableRole(IRole role)
		{
			RoleId = role.Id;
			Role = role;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			Role = guild.GetRole(RoleId);
		}

		public override string ToString()
		{
			return $"**Role:** `{Role.FormatRole()}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	/// <summary>
	/// Roles which are given back to users when they rejoin a guild.
	/// </summary>
	public class PersistentRole : ISetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong RoleId { get; }

		public PersistentRole(IUser user, IRole role)
		{
			UserId = user.Id;
			RoleId = role.Id;
		}

		public override string ToString()
		{
			return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
		}
		public string ToString(SocketGuild guild)
		{
			var user = guild.GetUser(UserId).FormatUser() ?? UserId.ToString();
			var role = guild.GetRole(RoleId).FormatRole() ?? RoleId.ToString();
			return $"**User:** `{user}`\n**Role:&& `{role}`";
		}
	}
}
