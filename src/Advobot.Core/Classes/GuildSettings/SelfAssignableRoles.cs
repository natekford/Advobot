using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Core.Classes.GuildSettings
{
	/// <summary>
	/// Groups self assignable roles together.
	/// </summary>
	public class SelfAssignableRoles : IGuildSetting
	{
		[JsonProperty]
		public int Group { get; }
		[JsonProperty("Roles")]
		private List<ulong> _RoleIds = new List<ulong>();
		[JsonIgnore]
		private Dictionary<ulong, IRole> _Roles = new Dictionary<ulong, IRole>();
		[JsonIgnore]
		public ImmutableList<IRole> Roles => _Roles.Values.ToImmutableList();

		public SelfAssignableRoles(int group)
		{
			Group = group;
		}

		public void AddRoles(IEnumerable<IRole> roles)
		{
			foreach (var role in roles)
			{
				if (!_Roles.ContainsKey(role.Id))
				{
					_Roles.Add(role.Id, role);
					_RoleIds.Add(role.Id);
				}
			}
		}
		public void RemoveRoles(IEnumerable<IRole> roles)
		{
			foreach (var role in roles)
			{
				_Roles.Remove(role.Id);
				_RoleIds.Remove(role.Id);
			}
		}
		public bool TryGetRole(ulong id, out IRole role)
		{
			return _Roles.TryGetValue(id, out role);
		}

		public void PostDeserialize(SocketGuild guild)
		{
			foreach (var roleId in _RoleIds)
			{
				var role = guild.GetRole(roleId);
				if (role == null)
				{
					_RoleIds.Remove(roleId);
					continue;
				}

				_Roles.Add(roleId, role);
			}
		}

		public override string ToString()
		{
			return String.Join("\n", _Roles.Select(x => $"**Role:** `{x.Value?.Format() ?? x.Key.ToString()}`"));
		}
		public string ToString(SocketGuild guild)
		{
			return String.Join("\n", _Roles.Select(x => $"**Role:** `{guild.GetRole(x.Key)?.Format() ?? x.Key.ToString()}`"));
		}
	}
}