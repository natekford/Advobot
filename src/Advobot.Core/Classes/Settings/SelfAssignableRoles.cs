using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Groups self assignable roles together.
	/// </summary>
	public class SelfAssignableRoles : IGuildSetting
	{
		/// <summary>
		/// The group number all the roles belong to.
		/// </summary>
		[JsonProperty]
		public int Group { get; }
		/// <summary>
		/// The roles in the group.
		/// </summary>
		[JsonIgnore]
		public ImmutableList<IRole> Roles => _Roles.Values.ToImmutableList();

		[JsonProperty("Roles")]
		private List<ulong> _RoleIds = new List<ulong>();
		[JsonIgnore]
		private Dictionary<ulong, IRole> _Roles = new Dictionary<ulong, IRole>();

		/// <summary>
		/// Creates an instance of selfassignableroles.
		/// </summary>
		/// <param name="group"></param>
		public SelfAssignableRoles(int group)
		{
			Group = group;
		}

		/// <summary>
		/// Adds the roles to the group.
		/// </summary>
		/// <param name="roles"></param>
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
		/// <summary>
		/// Removes the roles from the group.
		/// </summary>
		/// <param name="roles"></param>
		public void RemoveRoles(IEnumerable<IRole> roles)
		{
			foreach (var role in roles)
			{
				_Roles.Remove(role.Id);
				_RoleIds.Remove(role.Id);
			}
		}
		/// <summary>
		/// Attempts to get the role from the group.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="role"></param>
		/// <returns></returns>
		public bool TryGetRole(ulong id, out IRole role)
			=> _Roles.TryGetValue(id, out role);
		/// <summary>
		/// Uses the guild to get all the roles.
		/// </summary>
		/// <param name="guild"></param>
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

		/// <inheritdoc />
		public override string ToString()
			=> string.Join("\n", _Roles.Select(x => $"**Role:** `{x.Value?.Format() ?? x.Key.ToString()}`"));
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> string.Join("\n", _Roles.Select(x => $"**Role:** `{guild.GetRole(x.Key)?.Format() ?? x.Key.ToString()}`"));
	}
}